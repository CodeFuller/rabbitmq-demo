using Microsoft.Extensions.DependencyInjection;
using WorkQueue.Common.Interfaces;

namespace WorkQueue.Common
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddPayloadSerializer(this IServiceCollection services)
		{
			services.AddSingleton<IPayloadSerializer, PayloadSerializer>();

			return services;
		}

		public static IServiceCollection AddPayloadDeserializer(this IServiceCollection services)
		{
			services.AddSingleton<IPayloadDeserializer, PayloadSerializer>();

			return services;
		}
	}
}
