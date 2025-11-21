// filepath: c:\Users\alan\source\repos\AppNetCredenciales\.net-proyecto\BACKEND\LabNet\src\Espectaculos.WebApi\Services\RabbitMqService.cs
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
namespace Espectaculos.WebApi.Services;


public class RabbitMqService
{
    private readonly IConfiguration _configuration;

    public RabbitMqService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendMessage(string message)
{
    var hostName = _configuration["RabbitMQ:Host"] ?? throw new ArgumentNullException("RabbitMQ:Host is not configured");
    var port = int.TryParse(_configuration["RabbitMQ:Port"], out var parsedPort) ? parsedPort : throw new ArgumentNullException("RabbitMQ:Port is not configured or invalid");
    var userName = _configuration["RabbitMQ:Username"] ?? throw new ArgumentNullException("RabbitMQ:Username is not configured");
    var password = _configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ:Password is not configured");
    var queueName = _configuration["RabbitMQ:QueueName"] ?? throw new ArgumentNullException("RabbitMQ:QueueName is not configured");
    var dlqName = $"{queueName}-dlq"; // Nombre de la DLQ

    var factory = new ConnectionFactory
    {
        HostName = hostName,
        Port = port,
        UserName = userName,
        Password = password
    };

    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();


    // Declarar la DLQ
    channel.QueueDeclare(queue: dlqName, durable: true, exclusive: false, autoDelete: false, arguments: null);

    // Declarar la cola principal con soporte para DLQ
    channel.QueueDeclare(queue: queueName,
                         durable: true,
                         exclusive: false,
                         autoDelete: false,
                         arguments: new Dictionary<string, object>
                         {
                             { "x-dead-letter-exchange", "" }, // Enviar a la DLQ en caso de error
                             { "x-dead-letter-routing-key", dlqName },
                             { "x-max-length", 200 }
                         });

    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
}
}