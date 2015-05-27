using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace BoDi.Tests
{

    public interface IFancy{}
    public class ImFancy : IFancy {}
    public class ImFancier : IFancy {}
    public class ImFanciest : IFancy {}

    [TestFixture]
    public class ResolveAllTests
    {

        [Test]
        public void ShouldResolveTheRightNumberOfRegisteredTypes()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<ImFancy, IFancy>("fancy");
            container.RegisterTypeAs<ImFancier, IFancy>("fancier");
            container.RegisterTypeAs<ImFanciest, IFancy>("fanciest");

            // when
            var results = container.ResolveAll<IFancy>();

            // then
            Assert.AreEqual(3, results.Count());
        }

        [Test]
        public void ShouldResolveTheRightTypes()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<ImFancy, IFancy>("fancy");
            container.RegisterTypeAs<ImFancier, IFancy>("fancier");

            // when
            var results = container.ResolveAll<IFancy>();

            // then
            Assert.IsTrue(results.Contains(container.Resolve<IFancy>("fancy")));
            Assert.IsTrue(results.Contains(container.Resolve<IFancy>("fancier")));
        }

    }

}