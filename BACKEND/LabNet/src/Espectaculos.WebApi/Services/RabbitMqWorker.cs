using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Espectaculos.WebApi.Services;

public class RabbitMqWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqWorker> _logger;

    public RabbitMqWorker(IConfiguration configuration, ILogger<RabbitMqWorker> logger)
    {
        _configuration = configuration;
        _logger = logger;
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

        // ❌ NO declarar la cola aquí
        // La cola ya debe existir, creada por RabbitMqService

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                _logger.LogInformation($"Procesando mensaje: {message}");

                await Task.Delay(1000, stoppingToken);

                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el mensaje");

                // Esto manda a DLQ gracias a los argumentos ya declarados por el producer
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        channel.BasicConsume(
            queue: _configuration["RabbitMQ:QueueName"],
            autoAck: false,
            consumer: consumer
        );

        await Task.CompletedTask;
    }
}
