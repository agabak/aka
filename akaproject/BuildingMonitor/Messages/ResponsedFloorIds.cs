using System.Collections.Immutable;

namespace BuildingMonitor.Messages
{
    public sealed class ResponsedFloorIds
    {
        public long RequestId { get; }
        public ImmutableHashSet<string> Ids { get; }

        public ResponsedFloorIds(long requestId, ImmutableHashSet<string> ids)
        {
            RequestId = requestId;
            Ids = ids;
        }

    }
}
