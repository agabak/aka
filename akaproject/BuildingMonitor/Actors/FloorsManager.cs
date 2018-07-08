

using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using BuildingMonitor.Messages;
using System.Linq;

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
                case RequestRegisterTemperatureSensor m:
                    if (_floorIdsToActorRefs.TryGetValue(m.FloorId, out var existingSensorActorRef))
                    {
                        existingSensorActorRef.Forward(m);
                    }
                    else
                    {
                        var newSensorActor = Context.ActorOf(Floor.Props(m.FloorId), $"floor-{m.FloorId}");
                        _floorIdsToActorRefs.Add(m.FloorId, newSensorActor);
                        Context.Watch(newSensorActor);
                        newSensorActor.Forward(m);
                    }
                    break;
                case RequestFloorIds m:
                    Sender.Tell(new ResponsedFloorIds(m.RequestId, 
                                ImmutableHashSet.CreateRange(_floorIdsToActorRefs.Keys)));
                    break;
                case Terminated m:
                    var teminatedTemperatureSensorId =
                        _floorIdsToActorRefs.First(x => x.Value == m.ActorRef).Key;
                    _floorIdsToActorRefs.Remove(teminatedTemperatureSensorId);
                    break;
                default:
                        Unhandled(message);
                      break;
            }
        }

        public static Props Props() => Akka.Actor.Props.Create<FloorsManager>();
    }
}
