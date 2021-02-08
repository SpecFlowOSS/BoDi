using System.Linq;
using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class ResolveAllFromFactory : SingleContainerBenchmarkBase
    {
        [Benchmark(Baseline = true, Description = "v1.4")]
        public object Version_1_4()
        {
            // v1.4 returned a yet unresolved IEnumerable, so we need to force resolution
            return Container14.ResolveAll<IAllRegisteredFromFactory>().ToList();
        }

        [Benchmark(Description = "v1.BoDi_Concurrent_Dictionary_And_Lazy")]
        public object Version_1_BoDi_Concurrent_Dictionary_And_Lazy()
        {
            return Container1Concurrent_Dictionary_And_Lazy.ResolveAll<IAllRegisteredFromFactory>();
        }

        [Benchmark(Description = "Current")]
        public object CurrentVersion()
        {
            // current returns a yet unresolved IEnumerable, so we need to force resolution
            return ContainerCurrent.ResolveAll<IAllRegisteredFromFactory>().ToList();
        }
    }
}