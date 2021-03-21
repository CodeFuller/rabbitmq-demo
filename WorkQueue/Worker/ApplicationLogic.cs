using System;
using System.Threading;
using System.Threading.Tasks;
using CodeFuller.Library.Bootstrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WorkQueue.Common;
using WorkQueue.Common.Interfaces;

namespace Worker
{
	internal class ApplicationLogic : IApplicationLogic
	{
		private readonly Random random = new();

		private readonly IPayloadDeserializer payloadDeserializer;

		private readonly ILogger<ApplicationLogic> logger;

		private readonly RabbitMQSettings settings;

		public ApplicationLogic(IPayloadDeserializer payloadSerializer, ILogger<ApplicationLogic> logger, IOptions<RabbitMQSettings> options)
		{
			this.payloadDeserializer = payloadSerializer ?? throw new ArgumentNullException(nameof(payloadSerializer));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public Task<int> Run(string[] args, CancellationToken cancellationToken)
		{
			var factory = new ConnectionFactory
			{
				HostName = settings.Hostname,
			};

			logger.LogInformation("Creating connection ...");
			using var connection = factory.CreateConnection();

			logger.LogInformation("Creating channel ...");
			using var channel = connection.CreateModel();

			channel.QueueDeclare(queue: settings.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

			// Do not dispatch new message to a worker until it has processed the previous one.
			channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (_, ea) => ProcessMessage(channel, ea);

			logger.LogInformation("Consuming the messages. Press enter to exit.");
			channel.BasicConsume(queue: settings.QueueName, autoAck: false, consumer: consumer);

			Console.ReadLine();

			return Task.FromResult(0);
		}

		private void ProcessMessage(IModel channel, BasicDeliverEventArgs ea)
		{
			try
			{
				var payload = payloadDeserializer.Deserialize(ea.Body.ToArray());
				ProcessTask(payload);

				channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				logger.LogError(e, "Failed to process message");

				channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
			}
		}

		private void ProcessTask(TaskPayload payload)
		{
			logger.LogInformation("Processing task: {TaskNumber} {TaskDuration}", payload.TaskNumber, payload.Duration);

			Thread.Sleep(payload.Duration);

#pragma warning disable CA5394 // Do not use insecure randomness
			var fail = random.Next(5) == 0;
#pragma warning restore CA5394 // Do not use insecure randomness

			if (fail)
			{
				logger.LogWarning("The task processing has failed: {TaskNumber} {TaskDuration}", payload.TaskNumber, payload.Duration);
				throw new InvalidOperationException("The task processing has failed");
			}

			logger.LogInformation("The task processing has succeeded: {TaskNumber} {TaskDuration}", payload.TaskNumber, payload.Duration);
		}
	}
}
