﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Should;

namespace BoDi.Tests
{
    [TestFixture]
    public class RegisterFactoryDelegateTests
    {
        [Test]
        public void ShouldBeAbleToRegisterAFactoryDelegate()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFactoryAs<IInterface1>(() => new VerySimpleClass());

            // when

            var obj = container.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(VerySimpleClass), obj);
        }

        [Test]
        public void ShouldReturnTheSameIfResolvedTwice()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFactoryAs<IInterface1>(() => new VerySimpleClass());

            // when

            var obj1 = container.Resolve<IInterface1>();
            var obj2 = container.Resolve<IInterface1>();

            // then

            Assert.AreSame(obj1, obj2);
        }

        [Test]
        public void ShouldBeAbleToRegisterAFactoryDelegateWithDependencies()
        {
            // given
            var container = new ObjectContainer();
            var dependency = new VerySimpleClass();
            container.RegisterInstanceAs<IInterface1>(dependency);
            container.RegisterFactoryAs<IInterface3>(new Func<IInterface1, IInterface3>(if1 => new ClassWithSimpleDependency(if1)));

            // when

            var obj = container.Resolve<IInterface3>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(ClassWithSimpleDependency), obj);
            Assert.AreSame(dependency, ((ClassWithSimpleDependency)obj).Dependency);
        }

        [Test]
        public void ShouldBeAbleToRegisterAFactoryDelegateWithDependencyToTheContainer()
        {
            // given
            var container = new ObjectContainer();
            var dependency = new VerySimpleClass();
            container.RegisterInstanceAs<IInterface1>(dependency);
            container.RegisterFactoryAs<IInterface3>(c => new ClassWithSimpleDependency(c.Resolve<IInterface1>()));

            // when

            var obj = container.Resolve<IInterface3>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(ClassWithSimpleDependency), obj);
            Assert.AreSame(dependency, ((ClassWithSimpleDependency)obj).Dependency);
        }

        [Test]
        public void ShouldDisposeFactoryWhenRequested()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFactoryAs<IDisposableClass>(() => new DisposableClass1(), dispose: true);
            var obj = container.Resolve<IDisposableClass>();

            // when
            container.Dispose();

            // then
            Assert.IsTrue(obj.WasDisposed);
        }

        [Test]
        public void ShouldNotDisposeFactoryWhenDispositionIsntRequested()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFactoryAs<IDisposableClass>(() => new DisposableClass1(), dispose: false);
            var obj = container.Resolve<IDisposableClass>();

            // when
            container.Dispose();

            // then
            obj.WasDisposed.ShouldBeFalse();
        }

        [Test]
        public void ShouldNotDisposeFactoryWhenDispositionIsRequestedForANonDisposableFactory()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFactoryAs<IInterface1>(() => new SimpleClassWithDefaultCtor(), dispose: true);
            var obj = container.Resolve<IInterface1>();

            // when
            container.Dispose();
        }

        [Test]
        public void ShouldPreserveDefaultFactoryDispositionBehaviorWhichIsToNotDispose()
        {
            // given
            var container = (IObjectContainer)new ObjectContainer();
            container.RegisterFactoryAs<IDisposableClass>(c => new DisposableClass1());
            var obj = container.Resolve<IDisposableClass>();

            // when
            container.Dispose();

            // then
            obj.WasDisposed.ShouldBeFalse();
        }


        [Test]
        public void ShouldNotAttemptToDisposeFactoryMoreThanOnce()
        {
            // given
            var container = new ObjectContainer();
            var dependency = new VerySimpleClass();
            var instance = new OneTimeDisposableClass1();
            container.RegisterFactoryAs<IDisposableClass>(() => instance, dispose: true);
            var obj = container.Resolve<IDisposableClass>();
            obj = container.Resolve<IDisposableClass>();

            // when
            container.Dispose();

            // then
            obj.WasDisposed.ShouldBeTrue();
        }

        [Test]
        public void ShouldNotAttemptToReleaseANonDisposableFactory()
        {
            // given
            var container = new ObjectContainer();
            var dependency = new VerySimpleClass();
            container.RegisterFactoryAs<IInterface1>(() => new VerySimpleClass());
            var obj = container.Resolve<IInterface1>();

            // when
            container.Dispose();
        }

        [Test/*, ExpectedException(typeof(ObjectContainerException), ExpectedMessage = "Circular dependency", MatchType = MessageMatch.Contains)*/]
        [Ignore("dynamic circles not detected yet, this leads to stack overflow")]
        public void ShouldThrowExceptionForDynamicCircuarDepenencies()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterFactoryAs<ClassWithCircularDependency1>(c => new ClassWithCircularDependency1(c.Resolve<ClassWithCircularDependency2>()));

            // when 
            Assert.Throws<ObjectContainerException>(() => container.Resolve<ClassWithCircularDependency1>(), "Circular dependency");
        }

        [Test/*, ExpectedException(typeof(ObjectContainerException), ExpectedMessage = "Circular dependency", MatchType = MessageMatch.Contains)*/]
        public void ShouldThrowExceptionForStaticCircuarDepenencies()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterFactoryAs<ClassWithCircularDependency1>(new Func<ClassWithCircularDependency2, ClassWithCircularDependency1>(dep1 => new ClassWithCircularDependency1(dep1)));

            // when 
            Assert.Throws<ObjectContainerException>(() => container.Resolve<ClassWithCircularDependency1>(), "Circular dependency");
            
        }

        [Test/*, ExpectedException(typeof(ObjectContainerException), ExpectedMessage = "Circular dependency", MatchType = MessageMatch.Contains)*/]
        public void ShouldThrowExceptionForStaticCircuarDepenenciesWithMultipleFactoriesInPath()
        {
            // given

            var container = new ObjectContainer();
            container.RegisterFactoryAs<ClassWithCircularDependency1>(new Func<ClassWithCircularDependency2, ClassWithCircularDependency1>(dep2 => new ClassWithCircularDependency1(dep2)));
            container.RegisterFactoryAs<ClassWithCircularDependency2>(new Func<ClassWithCircularDependency1, ClassWithCircularDependency2>(dep1 => new ClassWithCircularDependency2(dep1)));

            // when 
            Assert.Throws<ObjectContainerException>(() => container.Resolve<ClassWithCircularDependency1>(), "Circular dependency");
        }
    }
}
