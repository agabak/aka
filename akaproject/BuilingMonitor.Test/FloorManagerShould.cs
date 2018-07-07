using Xunit;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using BuildingMonitor.Actors;
using BuildingMonitor.Messages;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace BuilingMonitor.Test
{
    public class FloorManagerShould:  TestKit
    {
        [Fact]
        public void ReturnNoFllorIdsWhenNewlyCreated()
        {
            var probe = CreateTestProbe();
            var manager = Sys.ActorOf(FloorsManager.Props());

            manager.Tell(new RequestFloorIds(1), probe.Ref);
            var received = probe.ExpectMsg<ResponsedFloorIds>();

            Assert.Equal(1, received.RequestId);
            Assert.Empty(received.Ids);
        }

        [Fact]
        public void RegisterNewFloorWhenDoesNotAlreadyExist()
        {
            var probe = CreateTestProbe();
            var manager = Sys.ActorOf(FloorsManager.Props());

            manager.Tell(new RequestRegisterTemperatureSensor(1, "a", "45"), probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>(x => x.RequestId == 1);

            manager.Tell(new RequestFloorIds(2), probe.Ref);
            var received = probe.ExpectMsg<ResponsedFloorIds>();

            Assert.Equal(2, received.RequestId);
            Assert.Single(received.Ids);
            Assert.Contains("a", received.Ids);
        }

        [Fact]
        public void ReuseExistingFloorWhenAlreadyExist()
        {
            var probe = CreateTestProbe();
            var manager = Sys.ActorOf(FloorsManager.Props());

            manager.Tell(new RequestRegisterTemperatureSensor(1, "a", "45"), probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>(x => x.RequestId == 1);

            manager.Tell(new RequestRegisterTemperatureSensor(2, "a", "900"), probe.Ref);
            probe.ExpectMsg<RespondSensorRegistered>(x => x.RequestId == 2);

            manager.Tell(new RequestFloorIds(3), probe.Ref);
            var received = probe.ExpectMsg<ResponsedFloorIds>();

            Assert.Equal(3, received.RequestId);
            Assert.Single(received.Ids);
            Assert.Contains("a", received.Ids);

        }

        [Fact]
        public async Task  ReturnFloorIdsOnlyFromActiveActor()
        {
            var probe = CreateTestProbe();
            var manager = Sys.ActorOf(FloorsManager.Props(), "FloorsManager");

            manager.Tell(new RequestRegisterTemperatureSensor(1, "a", "45"), probe.Ref);
            manager.Tell(new RequestRegisterTemperatureSensor(2, "b", "90"), probe.Ref);

            // stop one of the actors
            var firstFloor = await Sys.ActorSelection("akka://test/user/FloorsManager/floor-a")
                                       .ResolveOne(TimeSpan.FromSeconds(3));

            probe.Watch(firstFloor);
            firstFloor.Tell(PoisonPill.Instance);
            probe.ExpectTerminated(firstFloor);

            manager.Tell(new RequestFloorIds(1), probe.Ref);
            var received = probe.ExpectMsg<ResponsedFloorIds>();

            //Assert.Equal(, received.RequestId);
            Assert.Single(received.Ids);
            //Assert.Equal("b", received.Ids.First());
        }
    }
}
