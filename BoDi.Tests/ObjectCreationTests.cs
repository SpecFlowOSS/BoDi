using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BoDi.Tests
{
    [TestFixture]
    public class ObjectCreationTests
    {
        public class ConstructorTrackingClass : IInterface1
        {
            public static event Action<ConstructorTrackingClass> ConstructorCalled;

            public ConstructorTrackingClass()
            {
                if (ConstructorCalled != null)
                    ConstructorCalled(this);
            }
        }

        private ObjectContainer container;
        private List<ConstructorTrackingClass> calledConstructors;
        private Action<ConstructorTrackingClass> constructorTrackingClassOnConstructorCalled;

        [SetUp]
        public void Setup()
        {
            // given
            container = new ObjectContainer();
            container.RegisterTypeAs<IInterface1>(typeof(ConstructorTrackingClass));

            calledConstructors = new List<ConstructorTrackingClass>();
            constructorTrackingClassOnConstructorCalled = ctc => calledConstructors.Add(ctc);
            ConstructorTrackingClass.ConstructorCalled += constructorTrackingClassOnConstructorCalled;
        }

        public void TearDown()
        {
            if (constructorTrackingClassOnConstructorCalled != null)
                ConstructorTrackingClass.ConstructorCalled -= constructorTrackingClassOnConstructorCalled;
        }

        [Test]
        public void ShouldCreateObjectOnFirstResolve()
        {
            // given

            // when
            var obj = container.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(obj);
            CollectionAssert.AreEquivalent(new object[] { obj }, calledConstructors);
        }


        [Test]
        public void ShouldNotCreateObjectOnSecondResolve()
        {
            // given

            // when
            var obj1 = container.Resolve<IInterface1>();
            calledConstructors.Clear();
            var obj2 = container.Resolve<IInterface1>();

            // then
            CollectionAssert.IsEmpty(calledConstructors);
        }

        [Test]
        public void ShouldFireObjectCreatedEventWhenObjectIsCreated()
        {
            // given
            object objectCreated = null;
            container.ObjectCreated += o => objectCreated = o;

            // when
            var obj = container.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(objectCreated);
            Assert.AreSame(obj, objectCreated);
        }

        [Test]
        public void ShouldNotFireObjectCreatedEventOnSecondResolve()
        {
            // given
            object objectCreated = null;
            container.ObjectCreated += o => objectCreated = o;

            // when
            var obj1 = container.Resolve<IInterface1>();
            objectCreated = null;
            var obj2 = container.Resolve<IInterface1>();

            // then

            Assert.IsNull(objectCreated);
        }

        [Test]
        public void ShouldNotFireObjectCreatedEventOnResolvingInstanceRegistrations()
        {
            // given
            var obj = new ConstructorTrackingClass();
            container.RegisterInstanceAs<IInterface1>(obj);

            object objectCreated = null;
            container.ObjectCreated += o => objectCreated = o;

            // when
            container.Resolve<IInterface1>();

            // then
            Assert.IsNull(objectCreated);
        }

        [Test]
        public void ShouldNotFireObjectCreatedEventOnResolvingFactoryRegistrations()
        {
            // given
            container.RegisterFactoryAs<IInterface1>(() => new ConstructorTrackingClass());

            object objectCreated = null;
            container.ObjectCreated += o => objectCreated = o;

            // when
            container.Resolve<IInterface1>();

            // then
            Assert.IsNull(objectCreated);
        }
    }
}
