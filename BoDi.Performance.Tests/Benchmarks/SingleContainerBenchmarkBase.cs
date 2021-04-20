using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BoDi;

namespace BODi.Performance.Tests.Benchmarks
{
    [HtmlExporter]
    [MarkdownExporterAttribute.GitHub]
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
    public abstract class SingleContainerBenchmarkBase
    {
        protected internal ObjectContainer ContainerCurrent;
        protected internal BoDi1_4.ObjectContainer Container14;
        protected internal BoDi_Concurrent_Dictionary_And_Lazy.ObjectContainer Container1Concurrent_Dictionary_And_Lazy;

        [GlobalSetup]
        public void Setup()
        {
            Container14 = new BoDi1_4.ObjectContainer();
            Container14.RegisterFactoryAs(_ => new FactoryRegistered());
            Container14.RegisterTypeAs(typeof(GenericRegistered<>), typeof(IGenericRegistered<>));
            Container14.RegisterTypeAs<TypeRegistered, TypeRegistered>();
            Container14.RegisterTypeAs<AllRegistered1, IAllRegisteredFromType>();
            Container14.RegisterTypeAs<AllRegistered2, IAllRegisteredFromType>();
            Container14.RegisterTypeAs<AllRegistered3, IAllRegisteredFromType>();
            Container14.RegisterTypeAs<AllRegistered4, IAllRegisteredFromType>();
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered1());
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered2());
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered3());
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered4());


            Container1Concurrent_Dictionary_And_Lazy = new BoDi_Concurrent_Dictionary_And_Lazy.ObjectContainer();
            Container1Concurrent_Dictionary_And_Lazy.RegisterFactoryAs(_ => new FactoryRegistered());
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs(typeof(GenericRegistered<>), typeof(IGenericRegistered<>));
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<TypeRegistered, TypeRegistered>();
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<AllRegistered1, IAllRegisteredFromType>();
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<AllRegistered2, IAllRegisteredFromType>();
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<AllRegistered3, IAllRegisteredFromType>();
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<AllRegistered4, IAllRegisteredFromType>();
            Container1Concurrent_Dictionary_And_Lazy.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered1());
            Container1Concurrent_Dictionary_And_Lazy.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered2());
            Container1Concurrent_Dictionary_And_Lazy.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered3());
            Container1Concurrent_Dictionary_And_Lazy.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered4());

            ContainerCurrent = new ObjectContainer();
            ContainerCurrent.RegisterFactoryAs(_ => new FactoryRegistered());
            ContainerCurrent.RegisterTypeAs(typeof(GenericRegistered<>), typeof(IGenericRegistered<>));
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

        protected internal interface IGenericRegistered<T> { }

        protected internal class GenericRegistered<T> : IGenericRegistered<T> { }

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