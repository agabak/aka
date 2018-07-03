
namespace BuildingMonitor.Messages
{
    public sealed class RespondTempeture
    {
        public long RequestId { get;  }
        public double? Temperature { get;  }

        public RespondTempeture(long requestId, double? temperature)
        {
            RequestId = requestId;
            Temperature = temperature;
        }
    }
}
