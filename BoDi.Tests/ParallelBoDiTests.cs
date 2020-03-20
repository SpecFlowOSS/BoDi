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

        private static IObjectContainer[] Create(IBodiFactory factory)
        {
            var parent = factory.Create();
            parent.RegisterTypeAs<object, object>().InstancePerContext();

            IEnumerable<IObjectContainer> CreateChilds(int count)
            {
                for (var i = 1; i < count; i ++)
                {
                    yield return factory.Create(parent);
                }
            }

            return CreateChilds(50).ToArray();
        }

        [Test]
        public async Task ShouldWorkWhenCreateParallely()
        {
            var containers = Create(new BoDiSimpleFactory());

            var result = await Task.WhenAll(containers.Select(c => Task.Run(() => c.Resolve<object>())).ToArray());
            AssertResultsAreSame(result);
        }

        private static void AssertResultsAreSame(object[] result)
        {
            var first = result.First();
            foreach (var any in result)
            {
                Assert.AreSame(first, any);
            }
        }


        [Test]
        public void ShouldWorkWhenCreateSequentally()
        {
            var containers = Create(new BoDiSimpleFactory());
            var result = containers.Select(c => c.Resolve<object>()).ToArray();
            AssertResultsAreSame(result);
        }
    }
}
