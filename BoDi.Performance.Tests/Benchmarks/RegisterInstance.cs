using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class RegisterInstance : SingleEmptyContainerBenchmarkBase
    {
        private readonly TypeRegistered instance = new TypeRegistered();

        [Benchmark(Baseline = true, Description = "v1.4")]
        public void Version_1_4()
        {
            Container14.RegisterInstanceAs(instance);
        }

        [Benchmark(Description = "v1.BoDi_Concurrent_Dictionary_And_Lazy")]
        public void Version_1_BoDi_Concurrent_Dictionary_And_Lazy()
        {
            Container1Concurrent_Dictionary_And_Lazy.RegisterInstanceAs(instance);
        }

        [Benchmark(Description = "Current")]
        public void CurrentVersion()
        {
            ContainerCurrent.RegisterInstanceAs(instance);
        }
    }
}