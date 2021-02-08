using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class ResolveFromType : SingleContainerBenchmarkBase
    {
        [Benchmark(Baseline = true, Description = "v1.4")]
        public object Version_1_4()
        {
            return Container14.Resolve<TypeRegistered>();
        }

        [Benchmark(Description = "v1.BoDi_Concurrent_Dictionary_And_Lazy")]
        public object Version_1_BoDi_Concurrent_Dictionary_And_Lazy()
        {
            return Container1Concurrent_Dictionary_And_Lazy.Resolve<TypeRegistered>();
        }

        [Benchmark(Description = "Current")]
        public object CurrentVersion()
        {
            return ContainerCurrent.Resolve<TypeRegistered>();
        }
    }
}