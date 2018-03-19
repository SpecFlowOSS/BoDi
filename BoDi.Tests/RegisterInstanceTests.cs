using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace BoDi.Tests
{
    [TestFixture]
    public class RegisterInstanceTests
    {
        [Test/*, ExpectedException(typeof(ArgumentNullException))*/]
        public void ShouldThrowArgumentExceptionWhenCalledWithNull()
        {
            // given
            var container = new ObjectContainer();

            // when
            Assert.Throws<ArgumentNullException>(() => container.RegisterInstanceAs((IInterface1)null));
        }

        [Test]
        public void ShouldAllowOverrideRegistrationBeforeResolve()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();
            var instance = new SimpleClassWithDefaultCtor();

            // when 

            container.RegisterInstanceAs<IInterface1>(instance);

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
            var instance = new SimpleClassWithDefaultCtor();

            // when 

            container.RegisterInstanceAs<IInterface1>(instance);

            // then

            var obj = container.Resolve<IInterface1>();
            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(SimpleClassWithDefaultCtor), obj);
        }

        [Test/*, ExpectedException(typeof(ObjectContainerException))*/]
        public void ShouldNotAllowOverrideRegistrationAfterResolve()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();
            container.Resolve<IInterface1>();
            var instance = new SimpleClassWithDefaultCtor();

            // when 
            Assert.Throws<ObjectContainerException>( () => container.RegisterInstanceAs<IInterface1>(instance));
        }

        [Test/*, ExpectedException(typeof(ObjectContainerException))*/]
        public void ShouldNotAllowOverrideInstanceRegistrationAfterResolve()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterInstanceAs<IInterface1>(new VerySimpleClass());
            container.Resolve<IInterface1>();
            var instance = new SimpleClassWithDefaultCtor();

            // when 
            Assert.Throws<ObjectContainerException>(() => container.RegisterInstanceAs<IInterface1>(instance));
        }
    }
}
