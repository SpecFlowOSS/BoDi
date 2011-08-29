using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace BoDi.Tests
{
    [TestFixture]
    public class RegisterFromConfigTests
    {
        [Test]
        public void ShouldResolveFromDefaultSection()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterFromConfiguration();

            // when

            var obj = container.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(VerySimpleClass), obj);
        }

        [Test]
        public void ShouldResolveFromCustomSection()
        {
            // given
            var section = (TestConfigSection)ConfigurationManager.GetSection("testSection");
            Assert.IsNotNull(section);

            var container = new ObjectContainer();
            container.RegisterFromConfiguration(section.Dependencies);

            // when

            var obj = container.Resolve<IInterface1>();

            // then

            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(VerySimpleClass), obj);
        }
    }
}
