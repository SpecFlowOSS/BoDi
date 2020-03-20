using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoDi.Tests
{
    [TestFixture]
    public class ParallelBoDiTests
    {
        private interface IBodiFactory
        {
            IObjectContainer Create(IObjectContainer parent = null);

        }
        private class BoDiSimpleFactory : IBodiFactory
        {
            public IObjectContainer Create(IObjectContainer parent = null) => new ObjectContainer(parent);
        }

        private static IEnumerable<IObjectContainer> Create(IBodiFactory factory)
        {
            var parent = factory.Create();
            parent.RegisterTypeAs<object, object>();

            var left = factory.Create(parent);
            var right = factory.Create(parent);

            yield return left;
            yield return right;
        }

        [Test]
        public async Task ShouldWorkWhenCreateParallely()
        {
            var containers = Create(new BoDiSimpleFactory());

            var result = await Task.WhenAll(containers.Select(c => Task.Run(() => c.Resolve<object>())).ToArray());
            Assert.AreSame(result[0], result[1]);
        }

       

        [Test]
        public void ShouldWorkWhenCreateSequentally()
        {
            var containers = Create(new BoDiSimpleFactory());
            var result = containers.Select(c => c.Resolve<object>()).ToArray();
            Assert.AreSame(result[0], result[1]);
        }
    }
}
