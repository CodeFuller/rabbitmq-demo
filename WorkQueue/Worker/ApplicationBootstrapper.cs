using CodeFuller.Library.Bootstrap;
using CodeFuller.Library.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkQueue.Common;

namespace Worker
{
	internal class ApplicationBootstrapper : BasicApplicationBootstrapper<IApplicationLogic>
	{
		protected override void RegisterServices(IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<RabbitMQSettings>(settings => configuration.Bind("rabbitMQ", settings));

			services.AddPayloadDeserializer();

			services.AddSingleton<IApplicationLogic, ApplicationLogic>();
		}

		protected override void BootstrapLogging(ILoggerFactory loggerFactory, IConfiguration configuration)
		{
			loggerFactory.AddLogging(settings => configuration.Bind("logging", settings));
		}
	}
}
