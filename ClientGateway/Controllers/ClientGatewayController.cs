using System.Diagnostics.Metrics;
using System.Net;
using ClientGateway.Domain;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using static Confluent.Kafka.ConfigPropertyNames;

namespace ClientGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientGatewayController : ControllerBase
{
    private string BiometricsImportedTopicName = "BiometricsImported";
    private IProducer<string, Biometrics> _producer;
    private readonly ILogger<ClientGatewayController> _logger;

    public ClientGatewayController(IProducer<string,Biometrics> producer, ILogger<ClientGatewayController> logger)
    {
        _producer = producer;
        _logger = logger;
        logger.LogInformation("ClientGatewayController is Active.");
    }

    [HttpGet("Hello")]
    [ProducesResponseType(typeof(String), (int)HttpStatusCode.OK)]
    public String Hello()
    {
        _logger.LogInformation("Hello World");
        return "Hello World";
    }
    [HttpPost]
    [Route("Biometrics")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public async Task<AcceptedResult> RecordMeasurements([FromBody]Biometrics metrics)
    {
        _logger.LogInformation("Accepted biometrics");
        var message = new Message<string, Biometrics>() { Value = metrics };
       var result = await _producer.ProduceAsync(BiometricsImportedTopicName, message);
        if (result.Status == PersistenceStatus.Persisted)
        {
            Console.WriteLine($"Message delivered to {result.TopicPartitionOffset}");
        }
        else
        {
            Console.WriteLine($"Error: {result.Status}");
        }
        _producer.Flush();
        return Accepted("", metrics);
    }
}



