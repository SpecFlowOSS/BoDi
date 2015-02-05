using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Should;

namespace BoDi.Tests
{
    [TestFixture]
    public class DisposeTests
    {
        [Test]
        public void ContainerShouldBeDisposable()
        {
            var container = new ObjectContainer();

            container.ShouldImplement<IDisposable>();
        }

        [Test, ExpectedException(typeof(ObjectContainerException), ExpectedMessage = "disposed", MatchType = MessageMatch.Contains)]
        public void ContainerShouldThrowExceptionWhenDisposed()
        {
            var container = new ObjectContainer();
            container.Dispose();

            container.Resolve<IObjectContainer>();
        }

        [Test]
        public void ShouldDisposeCreatedObjects()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<DisposableClass1, IDisposableClass>();

            var obj = container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.ShouldBeTrue();
        }

        [Test]
        public void ShouldDisposeInstanceRegistrations()
        {
            var container = new ObjectContainer();
            var obj = new DisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj, dispose: true);

            container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.ShouldBeTrue();
        }

        [Test]
        public void ShouldNotDisposeObjectsRegisteredAsInstance()
        {
            var container = new ObjectContainer();
            var obj = new DisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj);

            container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.ShouldBeFalse();
        }

        [Test]
        public void ShouldNotDisposeObjectsFromBaseContainer()
        {
            var baseContainer = new ObjectContainer();
            baseContainer.RegisterTypeAs<DisposableClass1, IDisposableClass>();
            var container = new ObjectContainer(baseContainer);

            baseContainer.Resolve<IDisposableClass>();
            var obj = container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.ShouldBeFalse();
        }
    }
}
