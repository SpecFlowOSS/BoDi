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
    public abstract class SingleEmptyContainerBenchmarkBase
    {
        protected internal ObjectContainer ContainerCurrent;
        protected internal BoDi1_4.ObjectContainer Container14;
        protected internal BoDi_Concurrent_Dictionary_And_Lazy.ObjectContainer Container1Concurrent_Dictionary_And_Lazy;

        [GlobalSetup]
        public void Setup()
        {
            Container14 = new BoDi1_4.ObjectContainer();
            Container1Concurrent_Dictionary_And_Lazy = new BoDi_Concurrent_Dictionary_And_Lazy.ObjectContainer();
            ContainerCurrent = new ObjectContainer();
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