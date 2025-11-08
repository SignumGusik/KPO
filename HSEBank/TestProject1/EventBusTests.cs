using HSEBank.scr.Events;

namespace TestProject1
{
    public class EventBusTests
    {
        private class TestEvent : IEvent
        {
            public int Value { get; set; }
        }

        private class TestHandler : IEventHandler<TestEvent>
        {
            public TestEvent? Last { get; private set; }
            public void Handle(TestEvent ev) => Last = ev;
        }

        [Fact]
        public void Publish_CallsAllSubscribedHandlers()
        {
            var bus = new EventBus();
            var h1 = new TestHandler();
            var h2 = new TestHandler();

            bus.Subscribe(h1);
            bus.Subscribe(h2);

            var ev = new TestEvent { Value = 42 };

            bus.Publish(ev);

            Assert.NotNull(h1.Last);
            Assert.NotNull(h2.Last);
            Assert.Equal(42, h1.Last!.Value);
            Assert.Equal(42, h2.Last!.Value);
        }
    }
}