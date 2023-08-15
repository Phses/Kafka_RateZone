﻿using ClientGateway;
using ClientGateway.Controllers;
using ClientGateway.Domain;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// TODO: Load a config of type ProducerConfig from the "Kafka" section of the config:
builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("Kafka"));

builder.Services.Configure<SchemaRegistryConfig>(builder.Configuration.GetSection("SchemaRegistry"));


builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
{
    var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();

    return new CachedSchemaRegistryClient(config.Value);
});


// TODO: Register an IProducer of type <String, String>:
builder.Services.AddSingleton<IProducer<string, Biometrics>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
    var schemaRegistry = sp.GetRequiredService<ISchemaRegistryClient>();

    Console.WriteLine(schemaRegistry);

    return new ProducerBuilder<string, Biometrics>(config.Value).SetValueSerializer(new JsonSerializer<Biometrics>(schemaRegistry))
        .Build();
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();