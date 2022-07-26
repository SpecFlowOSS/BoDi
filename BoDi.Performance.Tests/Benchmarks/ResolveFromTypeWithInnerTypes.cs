using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class ResolveFromTypeWithInnerTypes : SingleContainerBenchmarkBase
    {
        [Benchmark(Baseline = true, Description = "v1.4")]
        public object Version_1_4()
        {
            return Container14.Resolve<OuterClass>();
        }

        [Benchmark(Description = "v1.BoDi_Concurrent_Dictionary_And_Lazy")]
        public object Version_1_BoDi_Concurrent_Dictionary_And_Lazy()
        {
            return Container1Concurrent_Dictionary_And_Lazy.Resolve<OuterClass>();
        }

        [Benchmark(Description = "Current")]
        public object CurrentVersion()
        {
            return ContainerCurrent.Resolve<OuterClass>();
        }
    }
}