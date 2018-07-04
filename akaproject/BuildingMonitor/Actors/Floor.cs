using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Actors
{
    public class Floor : UntypedActor
    {
        private readonly string _floorId;
        private Dictionary<string, IActorRef> _sensorIdToActorRefMap = 
                                                 new Dictionary<string, IActorRef>();
        public Floor(string floorId)
        {
            _floorId = floorId;
        }
   
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestRegisterTemperatureSensor m when m.FloorId == _floorId:
                    if(_sensorIdToActorRefMap.TryGetValue(m.SensorId, out var existingSensorActorRef))
                    {
                        existingSensorActorRef.Forward(m);
                    }
                    else
                    {
                        var newSensorActor = Context.ActorOf
                        (TemperatureSensor.Props(_floorId, m.SensorId), $"temperature-sensor-{m.SensorId}");

                        Context.Watch(newSensorActor);

                        _sensorIdToActorRefMap.Add(m.SensorId, newSensorActor);
                        newSensorActor.Forward(m);
                    }
                    break;
                case RequestTemperatureSensorIds m:
                    Sender.Tell((new RespondTemperatureSensorIds(m.RequestId,
                                                                 ImmutableHashSet.CreateRange(_sensorIdToActorRefMap.Keys))));
                    break;
                case Terminated m:
                    var teminatedTemperatureSensorId =
                        _sensorIdToActorRefMap.First(x => x.Value == m.ActorRef).Key;
                    _sensorIdToActorRefMap.Remove(teminatedTemperatureSensorId);
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        public static Props Props(string floorId) =>
            Akka.Actor.Props.Create(() =>new  Floor(floorId));
    }
}
