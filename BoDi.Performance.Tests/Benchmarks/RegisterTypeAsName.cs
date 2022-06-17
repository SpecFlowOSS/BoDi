using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class RegisterTypeAsName : SingleEmptyContainerBenchmarkBase
    {
        [Benchmark(Baseline = true, Description = "v1.4")]
        public void Version_1_4()
        {
            Container14.RegisterTypeAs<TypeRegistered, TypeRegistered>("Name");
        }

        [Benchmark(Description = "v1.BoDi_Concurrent_Dictionary_And_Lazy")]
        public void Version_1_BoDi_Concurrent_Dictionary_And_Lazy()
        {
            Container1Concurrent_Dictionary_And_Lazy.RegisterTypeAs<TypeRegistered, TypeRegistered>("Name");
        }

        [Benchmark(Description = "Current")]
        public void CurrentVersion()
        {
            ContainerCurrent.RegisterTypeAs<TypeRegistered, TypeRegistered>("Name");
        }
    }
}