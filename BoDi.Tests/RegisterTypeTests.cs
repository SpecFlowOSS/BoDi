using System;
using NUnit.Framework;

namespace BoDi.Tests
{
    [TestFixture]
    public class RegisterTypeTests
    {
        [Test]
        public void ShouldRegisterTypeDynamically()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<IInterface1>(typeof(VerySimpleClass));

            // when

            var obj = container.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(VerySimpleClass), obj);
        }

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

        [Test]
        public void ShouldRegisterGenericTypeDynamically()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs(typeof(SimpleGenericClass<>), typeof(IGenericInterface<>));

            // when
            var obj = container.Resolve<IGenericInterface<VerySimpleClass>>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(SimpleGenericClass<VerySimpleClass>), obj);
        }

        [Test]
        public void ShouldNotRegisterInvalidTypeMapping()
        {
            // given
            var container = new ObjectContainer();

            // then
            Assert.Catch<InvalidOperationException>(() => container.RegisterTypeAs(typeof(SimpleClassExtendingGenericInterface), typeof(IGenericInterface<>)));
        }

    }
}