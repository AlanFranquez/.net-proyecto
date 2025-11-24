using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.Mvc;
using MediatR; // Asegúrate de incluir MediatR
using Espectaculos.Application.Beneficios.Commands.CanjearBeneficio; // Comando para el canje

namespace Espectaculos.WebApi.Services;

public class RabbitMqWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqWorker> _logger;
    private readonly IMediator _mediator; // Inyectamos MediatR para manejar el comando
    private readonly HashSet<string> _processedMessages = new();

    public RabbitMqWorker(IConfiguration configuration, ILogger<RabbitMqWorker> logger, IMediator mediator)
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
            Port = int.Parse(_configuration["RabbitMQ:Port"]),
            UserName = _configuration["RabbitMQ:Username"],
            Password = _configuration["RabbitMQ:Password"]
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "usuarios-dlq", durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Declarar la cola principal con soporte para DLQ
        channel.QueueDeclare(queue: "usuarios",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "" }, // Enviar a la DLQ en caso de error
                { "x-dead-letter-routing-key", "usuarios-dlq" },
                { "x-max-length", 200 }
            }
        );

        // channel.QueueDeclare(
        //     queue: "usuarios",
        //     durable: true,
        //     exclusive: false,
        //     autoDelete: false,
        //     arguments: null
        // );
        // La cola ya debe existir, creada por RabbitMqService

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                var consumer = new EventingBasicConsumer(channel);

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

                        // Evitar procesar mensajes duplicados
                        if (_processedMessages.Contains(data.MessageId))
                        {
                            _logger.LogWarning($"Mensaje duplicado detectado: {data.MessageId}");
                            channel.BasicAck(ea.DeliveryTag, false);
                            return;
                        }

                        // Agregar el mensaje al conjunto de procesados
                        _processedMessages.Add(data.MessageId);

                        

                        // OK → ACK
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar el mensaje");

                        // NACK → No volver a reenviar el mensaje
                        channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                };

                channel.BasicConsume(
                    queue: _configuration["RabbitMQ:QueueName"],
                    autoAck: false,
                    consumer: consumer
                );

                _logger.LogInformation("RabbitMQ Worker escuchando mensajes...");

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
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

// Clase para deserializar el contenido del mensaje
public class CanjeBeneficioPayload
{
    public Guid BeneficioId { get; set; }
    public Guid UsuarioId { get; set; }
    public int Cantidad { get; set; }
}