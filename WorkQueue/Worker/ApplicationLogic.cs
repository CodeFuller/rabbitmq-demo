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
			consumer.Received += (_, ea) =>
			{
				var payload = payloadDeserializer.Deserialize(ea.Body.ToArray());
				ProcessTask(payload);

				channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
			};

			logger.LogInformation("Consuming the messages. Press enter to exit.");
			channel.BasicConsume(queue: settings.QueueName, autoAck: false, consumer: consumer);

			Console.ReadLine();

			return Task.FromResult(0);
		}

		private void ProcessTask(TaskPayload payload)
		{
			logger.LogInformation("Processing task: {TaskNumber} {TaskDuration}", payload.TaskNumber, payload.Duration);

			Thread.Sleep(payload.Duration);

			logger.LogInformation("The task was processed successfully: {TaskNumber} {TaskDuration}", payload.TaskNumber, payload.Duration);
		}
	}
}
