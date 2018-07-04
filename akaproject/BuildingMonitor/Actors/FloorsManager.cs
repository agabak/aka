

using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Actors
{
    public class FloorsManager : UntypedActor
    {
        private Dictionary<string,IActorRef>
               _floorIdsToActorRefs = new Dictionary<string, IActorRef>();
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestFloorIds m:
                    Sender.Tell(new ResponsedFloorIds(m.RequestId, 
                                ImmutableHashSet.CreateRange(_floorIdsToActorRefs.Keys)));
                    break;
                  default:
                        Unhandled(message);
                      break;
            }
        }

        public static Props Props() => Akka.Actor.Props.Create<FloorsManager>();
    }
}
