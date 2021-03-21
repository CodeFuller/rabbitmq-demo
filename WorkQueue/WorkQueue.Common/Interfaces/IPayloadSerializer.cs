namespace WorkQueue.Common.Interfaces
{
	public interface IPayloadSerializer
	{
		byte[] Serialize(TaskPayload payload);
	}
}
