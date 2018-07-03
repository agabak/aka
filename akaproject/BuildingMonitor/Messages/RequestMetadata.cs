
namespace BuildingMonitor.Messages
{
    public sealed class RequestMetadata
    {
        public long RequestId { get; set; }

        public RequestMetadata(long requestId)
        {
            RequestId = requestId;
        }
    }
}
