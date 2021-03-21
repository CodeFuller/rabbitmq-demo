using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeFuller.Library.Bootstrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Publisher
{
	internal class ApplicationLogic : IApplicationLogic
	{
		private readonly ILogger<ApplicationLogic> logger;

		private readonly RabbitMQSettings settings;

		public ApplicationLogic(ILogger<ApplicationLogic> logger, IOptions<RabbitMQSettings> options)
		{
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

			channel.ExchangeDeclare(exchange: settings.ExchangeName, type: ExchangeType.Fanout);

			logger.LogInformation("Publishing message ...");
			var body = Encoding.UTF8.GetBytes("Hello World!");
			channel.BasicPublish(exchange: settings.ExchangeName, routingKey: String.Empty, basicProperties: null, body: body);

			logger.LogInformation("The message was published successfully!");

			return Task.FromResult(0);
		}
	}
}
