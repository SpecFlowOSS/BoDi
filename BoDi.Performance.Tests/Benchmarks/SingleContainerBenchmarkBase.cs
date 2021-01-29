using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BoDi;

namespace BODi.Performance.Tests.Benchmarks
{
    [HtmlExporter]
    [MarkdownExporterAttribute.GitHub]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
    public abstract class SingleContainerBenchmarkBase
    {
        protected internal IObjectContainer ContainerCurrent;
        protected internal BoDi1_4.IObjectContainer Container14;
        protected internal BoDi1_Next_Flawed.IObjectContainer Container1NextFlawed;

        [GlobalSetup]
        public void Setup()
        {
            Container14 = new BoDi1_4.ObjectContainer();
            Container14.RegisterFactoryAs(_ => new FactoryRegistered());
            Container14.RegisterTypeAs<TypeRegistered, TypeRegistered>();
            Container14.RegisterTypeAs<AllRegistered1, IAllRegisteredFromType>();
            Container14.RegisterTypeAs<AllRegistered2, IAllRegisteredFromType>();
            Container14.RegisterTypeAs<AllRegistered3, IAllRegisteredFromType>();
            Container14.RegisterTypeAs<AllRegistered4, IAllRegisteredFromType>();
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered1());
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered2());
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered3());
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered4());


            Container1NextFlawed = new BoDi1_Next_Flawed.ObjectContainer();
            Container1NextFlawed.RegisterFactoryAs(_ => new FactoryRegistered());
            Container1NextFlawed.RegisterTypeAs<TypeRegistered, TypeRegistered>();
            Container1NextFlawed.RegisterTypeAs<AllRegistered1, IAllRegisteredFromType>();
            Container1NextFlawed.RegisterTypeAs<AllRegistered2, IAllRegisteredFromType>();
            Container1NextFlawed.RegisterTypeAs<AllRegistered3, IAllRegisteredFromType>();
            Container1NextFlawed.RegisterTypeAs<AllRegistered4, IAllRegisteredFromType>();
            Container1NextFlawed.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered1());
            Container1NextFlawed.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered2());
            Container1NextFlawed.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered3());
            Container1NextFlawed.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered4());

            ContainerCurrent = new ObjectContainer();
            ContainerCurrent.RegisterFactoryAs(_ => new FactoryRegistered());
            ContainerCurrent.RegisterTypeAs<TypeRegistered, TypeRegistered>();
            ContainerCurrent.RegisterTypeAs<AllRegistered1, IAllRegisteredFromType>();
            ContainerCurrent.RegisterTypeAs<AllRegistered2, IAllRegisteredFromType>();
            ContainerCurrent.RegisterTypeAs<AllRegistered3, IAllRegisteredFromType>();
            ContainerCurrent.RegisterTypeAs<AllRegistered4, IAllRegisteredFromType>();
            ContainerCurrent.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered1());
            ContainerCurrent.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered2());
            ContainerCurrent.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered3());
            ContainerCurrent.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered4());

        }

        protected internal class FactoryRegistered { }

        protected internal class TypeRegistered { }

        protected internal interface IAllRegisteredFromType { }

        protected internal interface IAllRegisteredFromFactory { }

        private class AllRegistered1 : IAllRegisteredFromType, IAllRegisteredFromFactory {}
        private class AllRegistered2 : IAllRegisteredFromType, IAllRegisteredFromFactory {}
        private class AllRegistered3 : IAllRegisteredFromType, IAllRegisteredFromFactory {}
        private class AllRegistered4 : IAllRegisteredFromType, IAllRegisteredFromFactory {}
    }
}