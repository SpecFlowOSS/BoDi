using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Should;

namespace BoDi.Tests
{
    [TestFixture]
    public class ContainerTests
    {
        [Test]
        public void ShouldListRegistrationsInToString()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();
            container.RegisterInstanceAs<IInterface1>(new SimpleClassWithDefaultCtor { Status = "instance1" });
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("one");
            container.RegisterInstanceAs<IInterface1>(new SimpleClassWithDefaultCtor { Status = "instance2" }, "two");
            container.RegisterInstanceAs<IInterface1>(new SimpleClassWithFailingToString(), "three");

            // when
            var result = container.ToString();
            Console.WriteLine(result);

            // then 
            result.ShouldContain("BoDi.IObjectContainer -> <self>");
            result.ShouldContain("BoDi.Tests.IInterface1 -> Instance: SimpleClassWithDefaultCtor: instance1");
            result.ShouldContain("BoDi.Tests.IInterface1('one') -> Type: BoDi.Tests.VerySimpleClass");
            result.ShouldContain("BoDi.Tests.IInterface1('two') -> Instance: SimpleClassWithDefaultCtor: instance2");
            result.ShouldContain("BoDi.Tests.IInterface1('three') -> Instance: simulated error");
        }
    }
}
