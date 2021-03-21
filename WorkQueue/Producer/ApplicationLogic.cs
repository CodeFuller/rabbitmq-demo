using System;
using System.Threading;
using System.Threading.Tasks;
using CodeFuller.Library.Bootstrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Producer.Settings;
using RabbitMQ.Client;
using WorkQueue.Common;
using WorkQueue.Common.Interfaces;

namespace Producer
{
	internal class ApplicationLogic : IApplicationLogic
	{
		private readonly Random random = new();

		private readonly IPayloadSerializer payloadSerializer;

		private readonly ILogger<ApplicationLogic> logger;

		private readonly ApplicationSettings settings;

		private WorkSettings WorkSettings => settings.Work;

		private RabbitMQSettings RabbitMQSettings => settings.RabbitMQ;

		public ApplicationLogic(IPayloadSerializer payloadSerializer, ILogger<ApplicationLogic> logger, IOptions<ApplicationSettings> options)
		{
			this.payloadSerializer = payloadSerializer ?? throw new ArgumentNullException(nameof(payloadSerializer));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public Task<int> Run(string[] args, CancellationToken cancellationToken)
		{
			var factory = new ConnectionFactory
			{
				HostName = RabbitMQSettings.Hostname,
			};

			logger.LogInformation("Creating connection ...");
			using var connection = factory.CreateConnection();

			logger.LogInformation("Creating channel ...");
			using var channel = connection.CreateModel();

			channel.QueueDeclare(queue: RabbitMQSettings.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

			var properties = channel.CreateBasicProperties();
			properties.Persistent = true;

			ProduceTasks(channel, properties);

			return Task.FromResult(0);
		}

		private void ProduceTasks(IModel channel, IBasicProperties properties)
		{
			logger.LogInformation("Producing {TasksNumber} work tasks ...", settings.Work.TasksNumber);

			for (var i = 1; i <= settings.Work.TasksNumber; ++i)
			{
				ProduceTask(channel, properties, i);
			}

			logger.LogInformation("Successfully produced {TasksNumber}", settings.Work.TasksNumber);
		}

		private void ProduceTask(IModel channel, IBasicProperties properties, int taskNumber)
		{
			var payload = new TaskPayload
			{
				TaskNumber = taskNumber,
				Duration = GetRandomTaskDuration(),
			};

			var data = payloadSerializer.Serialize(payload);

			channel.BasicPublish(String.Empty, RabbitMQSettings.QueueName, basicProperties: properties, body: data);
		}

		private TimeSpan GetRandomTaskDuration()
		{
#pragma warning disable CA5394 // Do not use insecure randomness
			var secondsNumber = random.Next((int)WorkSettings.MaxTaskDuration.TotalSeconds + 1);
#pragma warning restore CA5394 // Do not use insecure randomness

			return TimeSpan.FromSeconds(secondsNumber);
		}
	}
}
