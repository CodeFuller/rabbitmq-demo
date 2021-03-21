namespace WorkQueue.Common.Interfaces
{
	public interface IPayloadDeserializer
	{
		TaskPayload Deserialize(byte[] data);
	}
}
