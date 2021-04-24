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
        protected internal IObjectContainer ContainerCurrent;
        protected internal BoDi1_4.IObjectContainer Container14;
        protected internal BoDi_Concurrent_Dictionary_And_Lazy.IObjectContainer Container1Concurrent_Dictionary_And_Lazy;

        [Params(true, false)]
        public bool AsSingleton { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            Container14 = new BoDi1_4.ObjectContainer();
            Container14.RegisterFactoryAs(_ => new FactoryRegistered()).MaybeAsSingleton(AsSingleton);
            Container14.RegisterTypeAs<TypeRegistered, TypeRegistered>().MaybeAsSingleton(AsSingleton);
            Container14.RegisterTypeAs<AllRegistered1, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            Container14.RegisterTypeAs<AllRegistered2, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            Container14.RegisterTypeAs<AllRegistered3, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            Container14.RegisterTypeAs<AllRegistered4, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered1()).MaybeAsSingleton(AsSingleton);
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered2()).MaybeAsSingleton(AsSingleton);
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered3()).MaybeAsSingleton(AsSingleton);
            Container14.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered4()).MaybeAsSingleton(AsSingleton);


            Container1Concurrent_Dictionary_And_Lazy = new BoDi_Concurrent_Dictionary_And_Lazy.ObjectContainer();
            Container1Concurrent_Dictionary_And_Lazy.RegisterFactoryAs(_ => new FactoryRegistered()).MaybeAsSingleton(AsSingleton);
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<TypeRegistered, TypeRegistered>().MaybeAsSingleton(AsSingleton);
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<AllRegistered1, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<AllRegistered2, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<AllRegistered3, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<AllRegistered4, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            Container1Concurrent_Dictionary_And_Lazy.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered1()).MaybeAsSingleton(AsSingleton);
            Container1Concurrent_Dictionary_And_Lazy.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered2()).MaybeAsSingleton(AsSingleton);
            Container1Concurrent_Dictionary_And_Lazy.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered3()).MaybeAsSingleton(AsSingleton);
            Container1Concurrent_Dictionary_And_Lazy.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered4()).MaybeAsSingleton(AsSingleton);

            ContainerCurrent = new ObjectContainer();
            ContainerCurrent.RegisterFactoryAs(_ => new FactoryRegistered()).MaybeAsSingleton(AsSingleton);
            ContainerCurrent.RegisterTypeAs<TypeRegistered, TypeRegistered>().MaybeAsSingleton(AsSingleton);
            ContainerCurrent.RegisterTypeAs<AllRegistered1, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            ContainerCurrent.RegisterTypeAs<AllRegistered2, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            ContainerCurrent.RegisterTypeAs<AllRegistered3, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            ContainerCurrent.RegisterTypeAs<AllRegistered4, IAllRegisteredFromType>().MaybeAsSingleton(AsSingleton);
            ContainerCurrent.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered1()).MaybeAsSingleton(AsSingleton);
            ContainerCurrent.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered2()).MaybeAsSingleton(AsSingleton);
            ContainerCurrent.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered3()).MaybeAsSingleton(AsSingleton);
            ContainerCurrent.RegisterFactoryAs<IAllRegisteredFromFactory>(_ => new AllRegistered4()).MaybeAsSingleton(AsSingleton);
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

    internal static class RegistrationExtensions
    {
        public static void MaybeAsSingleton(this BoDi1_4.IStrategyRegistration strategyRegistration, bool asSingleton)
        {
            if (!asSingleton)
            {
                strategyRegistration.InstancePerDependency();
            }
        }

        public static void MaybeAsSingleton(this BoDi_Concurrent_Dictionary_And_Lazy.IStrategyRegistration strategyRegistration, bool asSingleton)
        {
            if (!asSingleton)
            {
                strategyRegistration.InstancePerDependency();
            }
        }

        public static void MaybeAsSingleton(this IStrategyRegistration strategyRegistration, bool asSingleton)
        {
            if (!asSingleton)
            {
                strategyRegistration.InstancePerDependency();
            }
        }
    }
}