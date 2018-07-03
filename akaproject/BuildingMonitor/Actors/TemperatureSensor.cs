using Akka.Actor;
using BuildingMonitor.Messages;

namespace BuildingMonitor.Actors
{
    public class TemperatureSensor: UntypedActor
    {
        private readonly string _floorId;
        private readonly string _sensoeId;
        private double? _lastTemperatureRecorded;

        public TemperatureSensor(string floorId, string sensorId)
        {
            _floorId = floorId;
            _sensoeId = sensorId;
        }
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestMetadata m:
                    Sender.Tell(new RespondMetadata(m.RequestId,_floorId, _sensoeId));
                    break;
                case RequestTempeture m:
                    Sender.Tell(new RespondTempeture(m.RequestId, _lastTemperatureRecorded));
                    break;
                case RequestUpdateTemperature m:
                    _lastTemperatureRecorded = m.Temperature;
                    Sender.Tell(new RespondTemperatureUpdated(m.RequestId));
                    break;
                case RequestRegisterTemperatureSensor m when 
                    m.FloorId == _floorId && m.SensorId == _sensoeId:
                    Sender.Tell(new RespondSensorRegistered(m.RequestId, Context.Self));
                    break;
                    default:
                        Unhandled(message);
                         break;
            }
        }

        public static Props Props(string floorId, string sensorId) =>
            Akka.Actor.Props.Create(() => new TemperatureSensor(floorId, sensorId)); 

    }
}
