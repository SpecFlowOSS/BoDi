using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MiniDi.Tests
{
    [TestFixture]
    public class ResolveTests
    {
        [Test]
        public void ShouldResolveVerySimpleClass()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when

            var obj = container.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(VerySimpleClass), obj);
        }

        [Test]
        public void ShouldResolveRegisteredInstance()
        {
            // given
            var instance = new VerySimpleClass();
            var container = new ObjectContainer();
            container.RegisterInstanceAs<IInterface1>(instance);

            // when

            var obj = container.Resolve<IInterface1>();

            // then

            Assert.AreSame(instance, obj);
        }

        [Test]
        public void ShouldResolveSimpleClassWithCtor()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>();

            // when

            var obj = container.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(SimpleClassWithDefaultCtor), obj);
            Assert.AreEqual("Initialized", ((SimpleClassWithDefaultCtor)obj).Status);
        }

        [Test]
        public void ShouldReturnTheSameIfResolvedTwice()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();

            // when

            var obj1 = container.Resolve<IInterface1>();
            var obj2 = container.Resolve<IInterface1>();

            // then

            Assert.AreSame(obj1, obj2);
        }

        [Test]
        public void ShouldResolveClassesWithoutRegstration()
        {
            // given
            var container = new ObjectContainer();

            // when

            var obj = container.Resolve<VerySimpleClass>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(VerySimpleClass), obj);
        }

        [Test]
        [ExpectedException(typeof(ObjectContainerException))]
        public void ShouldThrowErrorIfInterfaceCannotBeResolved()
        {
            // given
            var container = new ObjectContainer();

            // when

            container.Resolve<IInterface1>();
        }

        [Test]
        public void ShouldResolveClassWithSimpleDependency()
        {
            // given
            var dependency = new VerySimpleClass();
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithSimpleDependency, IInterface3>();
            container.RegisterInstanceAs<IInterface1>(dependency);

            // when

            var obj = container.Resolve<IInterface3>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(ClassWithSimpleDependency), obj);
            Assert.AreSame(dependency, ((ClassWithSimpleDependency)obj).Dependency);
        }


        [Test]
        public void ShouldResolveClassWithSimpleDependencies()
        {
            // given
            var dependency1 = new VerySimpleClass();
            var dependency2 = new AnotherVerySimpleClass();
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithSimpleDependencies, IInterface3>();
            container.RegisterInstanceAs<IInterface1>(dependency1);
            container.RegisterInstanceAs<IInterface2>(dependency2);

            // when

            var obj = container.Resolve<IInterface3>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(ClassWithSimpleDependencies), obj);
            Assert.AreSame(dependency1, ((ClassWithSimpleDependencies)obj).Dependency1);
            Assert.AreSame(dependency2, ((ClassWithSimpleDependencies)obj).Dependency2);
        }

        [Test]
        public void ShouldResolveClassWithDeeperDependencies()
        {
            // given
            var dependency1 = new VerySimpleClass();
            //var dependency2 = new ClassWithSimpleDependency();
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithDeeperDependency, IInterface4>();
            container.RegisterInstanceAs<IInterface1>(dependency1);
            container.RegisterTypeAs<ClassWithSimpleDependency, IInterface3>();

            // when

            var obj = container.Resolve<IInterface4>();
            var dependency2 = container.Resolve<IInterface3>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(ClassWithDeeperDependency), obj);
            Assert.AreSame(dependency2, ((ClassWithDeeperDependency)obj).Dependency);
            Assert.AreSame(dependency1, ((ClassWithSimpleDependency)((ClassWithDeeperDependency)obj).Dependency).Dependency);
        }

        [Test]
        public void ShouldResolveClassWithDeeperRedundantDependencies()
        {
            // given
            var dependency1 = new VerySimpleClass();
            //var dependency2 = new ClassWithSimpleDependency();
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithDeeperRedundantDependencies, IInterface4>();
            container.RegisterInstanceAs<IInterface1>(dependency1);
            container.RegisterTypeAs<ClassWithSimpleDependency, IInterface3>();

            // when

            var obj = container.Resolve<IInterface4>();
            var dependency2 = container.Resolve<IInterface3>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(ClassWithDeeperRedundantDependencies), obj);
            Assert.AreSame(dependency2, ((ClassWithDeeperRedundantDependencies)obj).Dependency1);
            Assert.AreSame(dependency1, ((ClassWithDeeperRedundantDependencies)obj).Dependency2);
            Assert.AreSame(dependency1, ((ClassWithSimpleDependency)((ClassWithDeeperRedundantDependencies)obj).Dependency1).Dependency);
        }

        [Test]
        public void ShouldResolveSameInstanceWhenTypeIsRegisteredAsTwoInterface()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<ClassWithTwoInterface, IInterface1>();
            container.RegisterTypeAs<ClassWithTwoInterface, IInterface2>();

            // when

            var obj1 = container.Resolve<IInterface1>();
            var obj2 = container.Resolve<IInterface2>();

            // then

            Assert.AreSame(obj1, obj2);
        }

        [Test]
        public void ShouldResolveRegisteredInstanceIfItsTypeIsAlsoRegistered()
        {
            // given
            var obj1 = new ClassWithTwoInterface();
            var container = new ObjectContainer();
            container.RegisterInstanceAs<IInterface1>(obj1);
            container.RegisterTypeAs<ClassWithTwoInterface, IInterface2>();

            // when

            var obj2 = container.Resolve<IInterface2>();

            // then

            Assert.AreSame(obj1, obj2);
        }

        [Test]
        public void ShouldResolveTheContainerItself()
        {
            // given

            var container = new ObjectContainer();

            // when 

            var obj = container.Resolve<ObjectContainer>();

            // then
            Assert.IsNotNull(obj);
            Assert.AreSame(container, obj);
        }

        [Test]
        public void ShouldResolveTheContainerItselfAsInterface()
        {
            // given

            var container = new ObjectContainer();

            // when 

            var obj = container.Resolve<IObjectContainer>();

            // then
            Assert.IsNotNull(obj);
            Assert.AreSame(container, obj);
        }

    }
}
