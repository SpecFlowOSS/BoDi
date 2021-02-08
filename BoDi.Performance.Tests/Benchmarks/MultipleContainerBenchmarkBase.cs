using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BoDi;

namespace BODi.Performance.Tests.Benchmarks
{
    [HtmlExporter]
    [MarkdownExporterAttribute.GitHub]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
    public abstract class MultipleContainerBenchmarkBase
    {
        protected internal IObjectContainer ContainerCurrentLevel1;
        protected internal IObjectContainer ContainerCurrentLevel2;
        protected internal IObjectContainer ContainerCurrentLevel3;
        protected internal IObjectContainer ContainerCurrentLevel4;

        [GlobalSetup]
        public void Setup()
        {
            ContainerCurrentLevel1 = new ObjectContainer();
            ContainerCurrentLevel2 = new ObjectContainer(ContainerCurrentLevel1);
            ContainerCurrentLevel3 = new ObjectContainer(ContainerCurrentLevel2);
            ContainerCurrentLevel4 = new ObjectContainer(ContainerCurrentLevel3);
            ContainerCurrentLevel1.RegisterTypeAs<TypeRegisteredLevel1, TypeRegisteredLevel1>();
            ContainerCurrentLevel2.RegisterTypeAs<TypeRegisteredLevel2, TypeRegisteredLevel2>();
            ContainerCurrentLevel3.RegisterTypeAs<TypeRegisteredLevel3, TypeRegisteredLevel3>();
            ContainerCurrentLevel4.RegisterTypeAs<TypeRegisteredLevel4, TypeRegisteredLevel4>();

        }

        protected internal class TypeRegisteredLevel1 { }
        protected internal class TypeRegisteredLevel2 { }
        protected internal class TypeRegisteredLevel3 { }
        protected internal class TypeRegisteredLevel4 { }

    }
}