using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MiniDi.Tests
{
    public interface IInterface1
    {
        
    }

    public interface IInterface2
    {
        
    }

    public interface IInterface3
    {
        
    }

    public interface IInterface4
    {
        
    }

    /// <summary>
    /// A very simple class without dependencies and ctor
    /// </summary>
    public class VerySimpleClass : IInterface1
    {
    }

    /// <summary>
    /// Another very simple class without dependencies and ctor
    /// </summary>
    public class AnotherVerySimpleClass : IInterface2
    {
    }

    /// <summary>
    /// A simple class without dependencies and but a default constructor
    /// </summary>
    public class SimpleClassWithDefaultCtor : IInterface1
    {
        public readonly string Status = "";

        public SimpleClassWithDefaultCtor()
        {
            Status = "Initialized";
        }
    }

    /// <summary>
    /// A clsss with a dependency that does not have further dependencies
    /// </summary>
    public class ClassWithSimpleDependency : IInterface3
    {
        public readonly IInterface1 Dependency;

        public ClassWithSimpleDependency(IInterface1 dependency)
        {
            Dependency = dependency;
        }
    }

    /// <summary>
    /// A clsss with a dependencies that does not have further dependencies
    /// </summary>
    public class ClassWithSimpleDependencies : IInterface3
    {
        public readonly IInterface1 Dependency1;
        public readonly IInterface2 Dependency2;

        public ClassWithSimpleDependencies(IInterface1 dependency1, IInterface2 dependency2)
        {
            Dependency1 = dependency1;
            Dependency2 = dependency2;
        }
    }

    /// <summary>
    /// A clsss with a dependency that has further dependencies
    /// </summary>
    public class ClassWithDeeperDependency : IInterface4
    {
        public readonly IInterface3 Dependency;

        public ClassWithDeeperDependency(IInterface3 dependency)
        {
            Dependency = dependency;
        }
    }

    /// <summary>
    /// A clsss with a dependency that has further dependencies and another dependency that was also required by the first
    /// </summary>
    public class ClassWithDeeperRedundantDependencies : IInterface4
    {
        public readonly IInterface3 Dependency1;
        public readonly IInterface1 Dependency2;

        public ClassWithDeeperRedundantDependencies(IInterface3 dependency1, IInterface1 dependency2)
        {
            Dependency2 = dependency2;
            Dependency1 = dependency1;
        }
    }


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

    }
}
