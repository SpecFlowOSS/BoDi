using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace BoDi.Tests
{
    [TestFixture]
    public class SubContainerTests
    {
        [Test]
        public void ShouldResolveFromBaseContainer()
        {
            // given
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<VerySimpleClass, IInterface1>();
            var container = new ObjectContainer(baseContainer);

            // when

            var obj = container.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(VerySimpleClass), obj);
        }

        [Test]
        public void ShouldResolveFromChildContainer()
        {
            // given
            var baseContainer = new ObjectContainer();
            var container = new ObjectContainer(baseContainer);
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when

            var obj = container.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(VerySimpleClass), obj);
        }

        [Test]
        public void ShouldBeAbleToOverrideBaseContainerRegistration()
        {
            // given
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();
            var container = new ObjectContainer(baseContainer);
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when

            var obj = container.Resolve<IInterface1>();
            var baseObj = baseContainer.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(VerySimpleClass), obj);
            Assert.AreNotEqual(obj, baseObj);
        }
    }
}
