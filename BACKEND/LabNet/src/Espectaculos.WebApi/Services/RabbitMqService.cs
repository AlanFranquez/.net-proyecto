using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
namespace Espectaculos.WebApi.Services;

public class RabbitMqService
{
    private readonly IConfiguration _config;
    private readonly HashSet<string> _declaredDlqs = new();

    public RabbitMqService(IConfiguration config)
    {
        _config = config;
    }

    // ---------------------------
    // 🔹 Helper para publicar en cualquier cola
    // ---------------------------
    private void PublishToQueue(string queueName, byte[] body)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"],
            Port = int.Parse(_config["RabbitMQ:Port"]),
            UserName = _config["RabbitMQ:Username"],
            Password = _config["RabbitMQ:Password"]
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var dlq = $"{queueName}-dlq";

        Console.WriteLine($"Intentando publicar en la cola: {queueName}");
        Console.WriteLine($"DLQ asociada: {dlq}");

        // Verificar si la DLQ ya fue declarada
        if (!_declaredDlqs.Contains(dlq))
        {
            Console.WriteLine($"Declarando DLQ: {dlq}");
            channel.QueueDeclare(dlq, true, false, false, null);
            _declaredDlqs.Add(dlq);
        }
        else
        {
            Console.WriteLine($"DLQ ya declarada: {dlq}");
        }

        // Declarar cola principal con DLQ
        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", dlq }
            }
        );

        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.MessageId = Guid.NewGuid().ToString();

        channel.BasicPublish("", queueName, props, body);
    }

    // ---------------------------
    // 🔹 Cola genérica
    // ---------------------------
   public void SendMessage(string queueName, string message)
{
    var payload = new MyMessage
    {
        MessageId = Guid.NewGuid().ToString(),
        Content = message
    };

    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
    PublishToQueue(queueName, body);
}

    public void SendToDlq(string queueName, string message)
{
    var payload = new MyMessage
    {
        MessageId = Guid.NewGuid().ToString(),
        Content = message
    };

    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
    PublishToQueue(queueName, body);
}

    // ---------------------------
    // 🔹 Cola para canjes
    // ---------------------------
    public void EnqueueCanje(Guid beneficioId, Guid usuarioId)
    {

        Console.WriteLine($"Encolando canje: BeneficioId={beneficioId}, UsuarioId={usuarioId}");
        var payload = new CanjearBeneficioMessage
        {
            BeneficioId = beneficioId,
            UsuarioId = usuarioId
        };
        Console.WriteLine($"ENVIANDO AL BODY: {JsonSerializer.Serialize(payload)}");
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

        PublishToQueue("beneficios.canjear", body);
    }

    public void EnqueueBeneficio(Guid beneficioId, Guid usuarioId)
    {
        var payload = new
        {
            Type = "CREAR_BENEFICIO",
            BeneficioId = beneficioId,
            UsuarioId = usuarioId
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

        PublishToQueue("beneficios.crear", body);

        
    }
}


public class CanjearBeneficioMessage
{
    public Guid BeneficioId { get; set; }
    public Guid UsuarioId { get; set; }
}
