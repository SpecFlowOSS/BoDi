#if !BODI_LIMITEDRUNTIME
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
#if !NETCOREAPP2_0 
        //Disable this tests, because of problem with dotnet test and app.configs - https://github.com/dotnet/corefx/issues/22101#partial-timeline-marker

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
#endif
    }
}
#endif