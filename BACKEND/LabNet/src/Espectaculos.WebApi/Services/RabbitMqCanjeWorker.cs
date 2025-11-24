using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MediatR;
using Espectaculos.Application.Beneficios.Commands.CreateBeneficio;
using Espectaculos.Application.Beneficios.Commands.UpdateBeneficio;
using Espectaculos.Application.Beneficios.Commands.CanjearBeneficio;
using Espectaculos.Application.Beneficios.Queries.ListBeneficios;
using Espectaculos.Application.Abstractions;
using Espectaculos.Application.Beneficios.Queries.GetBeneficioById;

namespace Espectaculos.WebApi.Services;



public class RabbitMqCanjeWorker : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<RabbitMqCanjeWorker> _logger;
    private readonly IServiceProvider _serviceProvider; // Use IServiceProvider for scoped services

    public RabbitMqCanjeWorker(IConfiguration config, ILogger<RabbitMqCanjeWorker> logger, IServiceProvider serviceProvider)
    {
        _config = config;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    var factory = new ConnectionFactory
    {
        HostName = _config["RabbitMQ:Host"],
        Port = int.Parse(_config["RabbitMQ:Port"]),
        UserName = _config["RabbitMQ:Username"],
        Password = _config["RabbitMQ:Password"]
    };

    const string queueName = "beneficios.canjear";
    const string dlqName = "beneficios.canjear-dlq";

    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();

    // Declarar DLQ
    channel.QueueDeclare(
        dlqName,
        durable: true,
        exclusive: false,
        autoDelete: false
    );

    // Declarar cola principal
    channel.QueueDeclare(
        queueName,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", dlqName }
        }
    );

    Console.WriteLine("RabbitMqCanjeWorker iniciado y escuchando en la cola de canjes...");

    var consumer = new EventingBasicConsumer(channel);

    consumer.Received += async (_, ea) =>
{
    try
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        var message = JsonSerializer.Deserialize<CanjearBeneficioMessage>(json);

        if (message == null)
        {
            _logger.LogError("Invalid message format");
            channel.BasicAck(ea.DeliveryTag, false);
            return;
        }

        _logger.LogInformation($"Processing canje: BeneficioId={message.BeneficioId}, UsuarioId={message.UsuarioId}");

        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Check if canje already exists
        var canjeExists = await uow.Canjes.ExistsAsync(
            x => x.BeneficioId == message.BeneficioId && x.UsuarioId == message.UsuarioId,
            stoppingToken
        );
        if (canjeExists)
        {
            _logger.LogWarning($"Canje Existe: BeneficioId={message.BeneficioId}, UsuarioId={message.UsuarioId}");
            channel.BasicAck(ea.DeliveryTag, false);
            return;
        }

        var cmd = new CanjearBeneficioCommand(message.BeneficioId, message.UsuarioId);
        await mediator.Send(cmd, stoppingToken);

        _logger.LogInformation($"Canje exitoso: BeneficioId={message.BeneficioId}, UsuarioId={message.UsuarioId}");
        channel.BasicAck(ea.DeliveryTag, false);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing message, sending to DLQ");
        channel.BasicNack(ea.DeliveryTag, false, false);
    }
};

    channel.BasicConsume(queueName, false, consumer);

    _logger.LogInformation("RabbitMqCanjeWorker escuchando mensajes...");

    await Task.CompletedTask; // ← mantener el worker vivo sin bloquearlo
}
}
