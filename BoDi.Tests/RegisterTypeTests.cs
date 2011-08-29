using NUnit.Framework;

namespace BoDi.Tests
{
    [TestFixture]
    public class RegisterTypeTests
    {
        [Test]
        public void ShouldAllowOverrideRegistrationBeforeResolve()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when 

            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();

            // then

            var obj = container.Resolve<IInterface1>();
            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(SimpleClassWithDefaultCtor), obj);
        }

        [Test]
        public void ShouldAllowOverrideInstanceRegistrationBeforeResolve()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterInstanceAs<IInterface1>(new VerySimpleClass());

            // when 

            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();

            // then

            var obj = container.Resolve<IInterface1>();
            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(SimpleClassWithDefaultCtor), obj);
        }

        [Test, ExpectedException(typeof(ObjectContainerException))]
        public void ShouldNotAllowOverrideRegistrationAfterResolve()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();
            container.Resolve<IInterface1>();

            // when 

            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();
        }

        [Test, ExpectedException(typeof(ObjectContainerException))]
        public void ShouldNotAllowOverrideInstanceRegistrationAfterResolve()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterInstanceAs<IInterface1>(new VerySimpleClass());
            container.Resolve<IInterface1>();

            // when 

            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();
        }
    }
}