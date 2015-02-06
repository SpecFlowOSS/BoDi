using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

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

    }
}
