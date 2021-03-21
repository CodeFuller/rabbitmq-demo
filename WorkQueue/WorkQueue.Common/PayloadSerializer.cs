using System.Text;
using System.Text.Json;
using WorkQueue.Common.Interfaces;

namespace WorkQueue.Common
{
	internal class PayloadSerializer : IPayloadSerializer, IPayloadDeserializer
	{
		private readonly JsonSerializerOptions serializeOptions = new()
		{
			WriteIndented = false,
			Converters =
			{
				new TimeSpanConverter(),
			},
		};

		public byte[] Serialize(TaskPayload payload)
		{
			var serializedData = JsonSerializer.Serialize(payload, serializeOptions);
			return Encoding.UTF8.GetBytes(serializedData);
		}

		public TaskPayload Deserialize(byte[] data)
		{
			var serializedData = Encoding.UTF8.GetString(data);
			return JsonSerializer.Deserialize<TaskPayload>(serializedData, serializeOptions);
		}
	}
}
