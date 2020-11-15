﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace BoDi.Tests
{
    [TestFixture]
    public class ConcurrencyTests
    {
        [Test]
        public void ShouldBeAbleToResolveFromMultipleThreadsWhenRegisteredAsType(
            [Values(RegistrationStrategy.PerContext, RegistrationStrategy.PerDependency)] RegistrationStrategy registrationStrategy,
            [Values(null, "Name")] string name)
        {
            try
            {
                IObjectContainer container = new ObjectContainer();
                var registration = container.RegisterTypeAs<BlockingObject, BlockingObject>(name);
                ApplyRegistrationStrategy(registration, registrationStrategy);

                void Resolve(object _)
                {
                    Assert.That(() => container.Resolve<BlockingObject>(name), Throws.Nothing);
                }

                var thread1 = new Thread(Resolve);
                var thread2 = new Thread(Resolve);
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                
                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Test]
        public void ShouldBeAbleToResolveFromMultipleThreadsWhenRegisteredAsFactory(
            [Values(RegistrationStrategy.PerContext, RegistrationStrategy.PerDependency)] RegistrationStrategy registrationStrategy,
            [Values(null, "Name")] string name)
        {
            try
            {
                IObjectContainer container = new ObjectContainer();
                container.RegisterFactoryAs(_ => new BlockingObject(), name);

                void Resolve(object _)
                {
                    Assert.That(() => container.Resolve<BlockingObject>(name), Throws.Nothing);
                }

                var thread1 = new Thread(Resolve);
                var thread2 = new Thread(Resolve);
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                
                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Test]
        public void ShouldBeAbleToResolveAllFromMultipleThreadsWhenRegisteredAsType(
            [Values(RegistrationStrategy.PerContext, RegistrationStrategy.PerDependency)] RegistrationStrategy registrationStrategy)
        {
            try
            {
                IObjectContainer container = new ObjectContainer();
                container.RegisterTypeAs<BlockingObject, BlockingObject>();

                void Resolve(object _)
                {
                    Assert.That(() => container.ResolveAll<BlockingObject>().ToList(), Throws.Nothing);
                }

                var thread1 = new Thread(Resolve);
                var thread2 = new Thread(Resolve);
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                
                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }

        [Test]
        public void ShouldBeAbleToResolveAllFromMultipleThreadsWhenRegisteredAsFactory(
            [Values(RegistrationStrategy.PerContext, RegistrationStrategy.PerDependency)] RegistrationStrategy registrationStrategy)
        {
            try
            {
                IObjectContainer container = new ObjectContainer();
                container.RegisterFactoryAs(_ => new BlockingObject());

                void Resolve(object _)
                {
                    Assert.That(() => container.ResolveAll<BlockingObject>().ToList(), Throws.Nothing);
                }

                var thread1 = new Thread(Resolve);
                var thread2 = new Thread(Resolve);
                thread1.Start();
                thread2.Start();

                // try to wait until both object constructions are in progress (may not happen if no threading issue)
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                BlockingObject.ObjectsCreated.WaitOne(ForHalfASecond);
                
                // allow constructors to finish
                BlockingObject.ConstructorBlockers[0].Set();
                BlockingObject.ConstructorBlockers[1].Set();

                // complete the threads
                if (!thread1.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
                if (!thread2.Join(ForHalfASecond))
                {
                    Assert.Fail("Unable to complete resolution");
                }
            }
            finally
            {
                BlockingObject.ResetBlockingEvents();
            }
        }
        
        private void ApplyRegistrationStrategy(IStrategyRegistration registration, RegistrationStrategy registrationStrategy)
        {
            switch (registrationStrategy)
            {
                case RegistrationStrategy.PerContext:
                    registration.InstancePerContext();
                    break;
                case RegistrationStrategy.PerDependency:
                    registration.InstancePerDependency();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(registrationStrategy), registrationStrategy, null);
            }
        }

        public enum RegistrationStrategy
        {
            PerContext,
            PerDependency
        }

        /// <summary>
        /// This object blocks on construction, to ensure control on multi-threading use cases
        /// </summary>
        private class BlockingObject
        {
            private const int MaxBlockingObjects = 2;

            public static List<EventWaitHandle> ConstructorBlockers;
            private static int _currentConstructorBlockerIndex;
            public static Semaphore ObjectsCreated;

            static BlockingObject()
            {
                ResetBlockingEvents();
            }

            public static void ResetBlockingEvents()
            {
                if (ConstructorBlockers != null)
                {
                    foreach (var evt in ConstructorBlockers)
                    {
                        evt.Set();
                    }
                }
                ConstructorBlockers = Enumerable.Repeat(new ManualResetEvent(false), MaxBlockingObjects).ToList<EventWaitHandle>();
                _currentConstructorBlockerIndex = -1;
                ObjectsCreated = new Semaphore(0, MaxBlockingObjects);
            }

            public BlockingObject()
            {
                var index = Interlocked.Increment(ref _currentConstructorBlockerIndex);
                var evt = ConstructorBlockers[index];
                ObjectsCreated.Release(1);
                evt.WaitOne();
            }
        }

        private static readonly TimeSpan ForHalfASecond = TimeSpan.FromMilliseconds(500);
    }
}