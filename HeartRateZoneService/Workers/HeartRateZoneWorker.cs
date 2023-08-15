using Confluent.Kafka;
using HeartRateZoneService.Domain;
using HeartRateZoneService.Domain.HeartRateZoneService;
using System.Reflection.Metadata;
using static Confluent.Kafka.ConfigPropertyNames;

namespace HeartRateZoneService.Workers;

public class HeartRateZoneWorker : BackgroundService
{
    private readonly IConsumer<string, Biometrics> _consumer;
    private readonly IProducer<string, HeartRateZoneReached> _producer;
    private readonly ILogger<HeartRateZoneWorker> _logger;
    private readonly string BiometricsImportedTopicName = "BiometricsImported";
    private readonly string HeartRateZoneReachedTopicName = "HeartRateZoneReached";
    private readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    public HeartRateZoneWorker(IConsumer<string,Biometrics> consumer, IProducer<string, HeartRateZoneReached> producer, ILogger<HeartRateZoneWorker> logger)
    {
        _consumer = consumer;
        _producer = producer;
        _logger = logger;
        _logger.LogInformation("HeartRateZoneWorker is Active.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _producer.InitTransactions(DefaultTimeout);
        _consumer.Subscribe(BiometricsImportedTopicName);
        while(!stoppingToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(stoppingToken);
            await HandleMessage(result.Message.Value, stoppingToken);
        }
        _consumer.Close();
    }
    
    protected virtual async Task HandleMessage(Biometrics biometrics, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Message Received: " + biometrics.DeviceId);

        //Get the offsets
        var offsets = _consumer.Assignment.
            Select(topicPartition => 
            new TopicPartitionOffset(topicPartition, _consumer.Position(topicPartition)
            ));

        _producer.BeginTransaction();
        //Send offsets to producer
        _producer.SendOffsetsToTransaction(offsets, _consumer.ConsumerGroupMetadata, DefaultTimeout);

        List<HeartRateZoneReached> heartRateZoneReacheds = biometrics.HeartRates
            .Where(hr => hr.GetHeartRateZone(biometrics.MaxHeartRate) != HeartRateZone.None)
            .Select(hr => new HeartRateZoneReached(
                biometrics.DeviceId, hr.GetHeartRateZone(biometrics.MaxHeartRate), hr.DateTime, hr.Value, biometrics.MaxHeartRate))
            .ToList();

        try
        {
            await Task.WhenAll(biometrics.HeartRates
                .Where(hr => hr.GetHeartRateZone(biometrics.MaxHeartRate) != HeartRateZone.None)
                .Select(hr =>
                {
                    var zone = hr.GetHeartRateZone(biometrics.MaxHeartRate);

                    var heartRateZoneReached = new HeartRateZoneReached(
                        biometrics.DeviceId,
                        zone,
                        hr.DateTime,
                        hr.Value,
                        biometrics.MaxHeartRate
                    );

                    var message = new Message<String, HeartRateZoneReached>
                    {
                        Key = biometrics.DeviceId.ToString(),
                        Value = heartRateZoneReached
                    };

                    return _producer.ProduceAsync(HeartRateZoneReachedTopicName, message, cancellationToken);
                }));

            _producer.CommitTransaction();
        }
        catch (Exception ex)
        {
            _producer.AbortTransaction();
            throw new Exception("Transaction Failed", ex);
        }
    }
}
