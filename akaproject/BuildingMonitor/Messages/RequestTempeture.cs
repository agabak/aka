
namespace BuildingMonitor.Messages
{
    public sealed class RequestTempeture
    {
        public long RequestId { get; set; }

        public RequestTempeture(long requestId)
        {
            RequestId = requestId;
        }
    }
}
