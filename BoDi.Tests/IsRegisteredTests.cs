using NUnit.Framework;

namespace BoDi.Tests
{
    [TestFixture]
    public class IsRegisteredTests
    {
        [Test]
        public void ShouldReturnFalseIfInterfaceNotRegistered()
        {
            // given

            var container = new ObjectContainer();

            // then

            bool isRegistered = container.IsRegistered<IInterface1>();

            Assert.IsFalse(isRegistered);
        }

        [Test]
        public void ShouldReturnFalseIfTypeNotRegistered()
        {
            // given

            var container = new ObjectContainer();

            // then

            bool isRegistered = container.IsRegistered<VerySimpleClass>();

            Assert.IsFalse(isRegistered);
        }

        [Test]
        public void ShouldReturnTrueIfInterfaceRegistered()
        {
            // given

            var container = new ObjectContainer();

            // when 

            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // then

            bool isRegistered = container.IsRegistered<IInterface1>();
            
            Assert.IsTrue(isRegistered);
        }

        [Test]
        public void ShouldReturnTrueIfTypeRegistered()
        {
            // given

            var container = new ObjectContainer();

            // when 

            container.RegisterInstanceAs(new SimpleClassWithDefaultCtor());

            // then

            bool isRegistered = container.IsRegistered<SimpleClassWithDefaultCtor>();

            Assert.IsTrue(isRegistered);
        }

        [Test]
        public void ShouldReturnTrueIfInterfaceRegisteredInBaseContainer()
        {
            // given

            var baseContainer = new ObjectContainer();
            var container = new ObjectContainer(baseContainer);

            // when 

            baseContainer.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // then

            bool isRegistered = container.IsRegistered<IInterface1>();

            Assert.IsTrue(isRegistered);
        }
    }
}
