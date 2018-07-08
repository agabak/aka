using Xunit;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;

namespace BuilingMonitor.Test
{
    public class FloorShould:TestKit
    {
        [Fact]
        public void RegiterNewTemperatureSensorsWhenDoesNotExist()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Props("a"));
            floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"), probe.Ref);

            var received = probe.ExpectMsg<RespondSensorRegistered>();
            Assert.Equal(1, received.RequestId);

            var sensorActor = probe.LastSender;

            sensorActor.Tell(new RequestUpdateTemperature(42, 100), probe.Ref);
            probe.ExpectMsg<RespondTemperatureUpdated>();
        }

        [Fact]
        public void ReturnExistTemperatureSensorsWhenReRegisteringTheSameSensor()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Props("a"));

            floor.Tell(new RequestRegisterTemperatureSensor(1, "a", "42"), probe.Ref);
            var received = probe.ExpectMsg<RespondSensorRegistered>();
            Assert.Equal(1, received.RequestId);

            var firstSensorActor = probe.LastSender;

            floor.Tell(new RequestRegisterTemperatureSensor(2, "a", "42"), probe.Ref);
            received = probe.ExpectMsg<RespondSensorRegistered>();
            Assert.Equal(2, received.RequestId);

            var secondSensorActor = probe.LastSender;

            Assert.Equal(firstSensorActor, secondSensorActor);
        }

        [Fact]
        public void NotRegisterSensorWhenMisMatchedFloor()
        {
            var probe = CreateTestProbe();
            var eventStreamProbe = CreateTestProbe();

            Sys.EventStream.Subscribe(eventStreamProbe, typeof(Akka.Event.UnhandledMessage));
            var foor = Sys.ActorOf(Floor.Props("a"));

            foor.Tell(new RequestRegisterTemperatureSensor(1, "b", "42"), probe);
            probe.ExpectNoMsg();

            var unhundle = eventStreamProbe.ExpectMsg<Akka.Event.UnhandledMessage>();

            Assert.IsType<RequestRegisterTemperatureSensor>(unhundle.Message);
            Assert.Equal(foor, unhundle.Recipient);
        }

        [Fact]
        public void ReturnAllTemperatureSensorsIds()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Props("a"));

            floor.Tell(new RequestRegisterTemperatureSensor(1,"a","42"),probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>();

            floor.Tell(new RequestRegisterTemperatureSensor(2,"a","90"),probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>();

            floor.Tell(new RequestTemperatureSensorIds(1),probe.Ref);
            var response = probe.ExpectMsg<RespondTemperatureSensorIds>();

            Assert.Equal(2,response.Ids.Count);
            Assert.Contains("42", response.Ids);
            Assert.Contains("90", response.Ids);
        }

        [Fact]
        public void ReturnEmptyListOfTemperatureSensorsIdsIfNoneExists()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Props("a"));

            floor.Tell(new RequestTemperatureSensorIds(1), probe.Ref);
            var response = probe.ExpectMsg<RespondTemperatureSensorIds>();

            Assert.Equal(0, response.Ids.Count);
        }

        [Fact]
        public void ReturnTemperatureSensorsIdsOnlyFromActiveActors()
        {
            var probe = CreateTestProbe();
            var floor = Sys.ActorOf(Floor.Props("a"));

            floor.Tell(new RequestRegisterTemperatureSensor(1,"a","42"),probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>();
            var firstSensorAdded = probe.LastSender;

            floor.Tell(new RequestRegisterTemperatureSensor(2, "a", "90"), probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>();
            var secondSensorAdded = probe.LastSender;

            // stop one of the actor
            probe.Watch(firstSensorAdded);
            firstSensorAdded.Tell(PoisonPill.Instance);
            probe.ExpectTerminated(firstSensorAdded);

            floor.Tell(new RequestTemperatureSensorIds(1), probe.Ref);
            var response = probe.ExpectMsg<RespondTemperatureSensorIds>();

            Assert.Equal(1, response.Ids.Count);
            Assert.Contains("90", response.Ids);
            Assert.Single(response.Ids);
        }
    }
}
