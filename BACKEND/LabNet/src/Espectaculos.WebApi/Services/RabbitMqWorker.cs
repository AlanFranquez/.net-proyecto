using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Espectaculos.Application.Beneficios.Commands.CanjearBeneficio;

namespace Espectaculos.WebApi.Services;

public class RabbitMqWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqWorker> _logger;
    private readonly IMediator _mediator;
    private readonly HashSet<string> _processedMessages = new();

    public RabbitMqWorker(
        IConfiguration configuration,
        ILogger<RabbitMqWorker> logger,
        IMediator mediator)
    {
        _configuration = configuration;
        _logger = logger;
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"],
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:Username"],
            Password = _configuration["RabbitMQ:Password"],

            // Needed if you want an async handler that RabbitMQ awaits properly
            DispatchConsumersAsync = true
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // DLQ
                channel.QueueDeclare(
                    queue: "usuarios-dlq",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Main queue with DLQ support
                channel.QueueDeclare(
                    queue: "usuarios",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", "" },
                        { "x-dead-letter-routing-key", "usuarios-dlq" },
                        { "x-max-length", 200 }
                    });

                // Process one at a time (avoids concurrency issues with HashSet)
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var data = JsonSerializer.Deserialize<MyMessage>(json);

                        if (data == null)
                        {
                            _logger.LogWarning("Mensaje inválido recibido");
                            channel.BasicAck(ea.DeliveryTag, false);
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(data.MessageId))
                        {
                            _logger.LogWarning("Mensaje sin MessageId recibido");
                            channel.BasicAck(ea.DeliveryTag, false);
                            return;
                        }

                        // Avoid duplicates
                        if (_processedMessages.Contains(data.MessageId))
                        {
                            _logger.LogWarning($"Mensaje duplicado detectado: {data.MessageId}");
                            channel.BasicAck(ea.DeliveryTag, false);
                            return;
                        }

                        _processedMessages.Add(data.MessageId);

                        // Your Content should contain the payload
                        var payload = JsonSerializer.Deserialize<CanjeBeneficioPayload>(data.Content);

                        if (payload == null)
                        {
                            _logger.LogWarning("Payload inválido en Content");
                            channel.BasicNack(ea.DeliveryTag, false, false);
                            return;
                        }

                        // Your command ONLY takes 2 params
                        var command = new CanjearBeneficioCommand(
                            payload.BeneficioId,
                            payload.UsuarioId
                        );

                        await _mediator.Send(command, stoppingToken);

                        // OK -> ACK
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar el mensaje");
                        // NACK without requeue -> goes to DLQ
                        channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                };

                channel.BasicConsume(
                    queue: _configuration["RabbitMQ:QueueName"] ?? "usuarios",
                    autoAck: false,
                    consumer: consumer
                );

                _logger.LogInformation("RabbitMQ Worker escuchando mensajes...");

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // service stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en RabbitMQ Worker. Reintentando en 5s...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}

public class MyMessage
{
    public string MessageId { get; set; }
    public string Content { get; set; }
}

// Payload inside Content
public class CanjeBeneficioPayload
{
    public Guid BeneficioId { get; set; }
    public Guid UsuarioId { get; set; }
    public int Cantidad { get; set; }
}
