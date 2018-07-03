
namespace BuildingMonitor.Messages
{
    public sealed  class RespondMetadata
    {
        public long RequsteId { get; }
        public string FloorId { get; }
        public string SensorId { get;  }

        public RespondMetadata(long requsteId, string floorId, string sensorId)
        {
            RequsteId = requsteId;
            FloorId = floorId;
            SensorId = sensorId;
        }
    }
}
