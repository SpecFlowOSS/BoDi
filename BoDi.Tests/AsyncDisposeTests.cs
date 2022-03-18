using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Should;

namespace BoDi.Tests
{
#if ASYNC_DISPOSE
    [TestFixture]
    public class AsyncDisposeTests
    {
        [Test]
        public void ContainerShouldBeAsyncDisposable()
        {
            var container = new ObjectContainer();

            container.ShouldImplement<IAsyncDisposable>();
        }

        [Test/*ExpectedException(typeof(ObjectContainerException), ExpectedMessage = "disposed", MatchType = MessageMatch.Contains)*/]
        public async Task ContainerShouldThrowExceptionWhenDisposedAsynchronously()
        {
            var container = new ObjectContainer();
            await container.DisposeAsync();

            Assert.Throws<ObjectContainerException>(() => container.Resolve<IObjectContainer>(), "Object container disposed");
        }

        [Test]
        public void ShouldDisposeAsyncCreatedObjects()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<AsyncDisposableClass1, IDisposableClass>();

            var obj = container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.ShouldBeTrue();
        }

        [Test]
        public async Task ShouldDisposeCreatedObjectsOnDisposeAsync()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<DisposableClass1, IDisposableClass>();

            var obj = container.Resolve<IDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.ShouldBeTrue();
        }

        [Test]
        public void ShouldPreferSyncOverAsyncDisposeAsyncCreatedObjectsOnSyncDispose()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<SyncAndAsyncDisposableClass1, IDisposableOptionallyAsyncClass>();

            var obj = container.Resolve<IDisposableOptionallyAsyncClass>();

            container.Dispose();

            obj.WasDisposed.ShouldBeTrue();
            obj.DisposalWasAsync.ShouldBeFalse();
        }

        [Test]
        public async Task ShouldPreferAsyncOverSyncDisposeAsyncCreatedObjectsOnAsyncDispose()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<SyncAndAsyncDisposableClass1, IDisposableOptionallyAsyncClass>();

            var obj = container.Resolve<IDisposableOptionallyAsyncClass>();

            await container.DisposeAsync();

            obj.WasDisposed.ShouldBeTrue();
            obj.DisposalWasAsync.ShouldBeTrue();
        }

        [Test]
        public async Task ShouldDisposeAsyncCreatedObjectsOnDisposeAsync()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<AsyncDisposableClass1, IDisposableClass>();

            var obj = container.Resolve<IDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.ShouldBeTrue();
        }

        [Test]
        public void ShouldAsyncDisposeInstanceRegistrations()
        {
            var container = new ObjectContainer();
            var obj = new AsyncDisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj, dispose: true);

            container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.ShouldBeTrue();
        }

        [Test]
        public async Task ShouldAsyncDisposeInstanceRegistrationsOnDisposeAsync()
        {
            var container = new ObjectContainer();
            var obj = new AsyncDisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj, dispose: true);

            container.Resolve<IDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.ShouldBeTrue();
        }

        [Test]
        public void ShouldPreferSyncDisposeOverAsyncForInstanceRegistrationsOnSyncDispose()
        {
            var container = new ObjectContainer();
            var obj = new SyncAndAsyncDisposableClass1();
            container.RegisterInstanceAs<IDisposableOptionallyAsyncClass>(obj, dispose: true);

            container.Resolve<IDisposableOptionallyAsyncClass>();

            container.Dispose();

            obj.WasDisposed.ShouldBeTrue();
            obj.DisposalWasAsync.ShouldBeFalse();
        }

        [Test]
        public async Task ShouldPreferAsyncDisposeOverSyncForInstanceRegistrationsOnAsyncDispose()
        {
            var container = new ObjectContainer();
            var obj = new SyncAndAsyncDisposableClass1();
            container.RegisterInstanceAs<IDisposableOptionallyAsyncClass>(obj, dispose: true);

            container.Resolve<IDisposableOptionallyAsyncClass>();

            await container.DisposeAsync();

            obj.WasDisposed.ShouldBeTrue();
            obj.DisposalWasAsync.ShouldBeTrue();
        }

        [Test]
        public void ShouldNotDisposeAsyncDisposableObjectsRegisteredAsInstance()
        {
            var container = new ObjectContainer();
            var obj = new AsyncDisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj);

            container.Resolve<IDisposableClass>();

            container.Dispose();

            obj.WasDisposed.ShouldBeFalse();
        }

        [Test]
        public async Task ShouldNotDisposeObjectsRegisteredAsInstanceOnDisposeAsync()
        {
            var container = new ObjectContainer();
            var obj = new DisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj);

            container.Resolve<IDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.ShouldBeFalse();
        }

        [Test]
        public async Task ShouldNotDisposeAsyncDisposableObjectsRegisteredAsInstanceOnDisposeAsync()
        {
            var container = new ObjectContainer();
            var obj = new AsyncDisposableClass1();
            container.RegisterInstanceAs<IDisposableClass>(obj);

            container.Resolve<IDisposableClass>();

            await container.DisposeAsync();

            obj.WasDisposed.ShouldBeFalse();
        }
    }
#endif
}
