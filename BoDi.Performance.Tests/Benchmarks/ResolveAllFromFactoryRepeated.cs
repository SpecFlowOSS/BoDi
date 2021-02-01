using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class ResolveAllFromFactoryRepeated : SingleContainerBenchmarkBase
    {
        [Params(100)]
        public int Repetitions { get; set; }

        [Benchmark(Baseline = true, Description = "v1.4")]
        public object Version_1_4()
        {
            List<IAllRegisteredFromFactory> obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                // v1.4 returned a yet unresolved IEnumerable, so we need to force resolution
                obj = Container14.ResolveAll<IAllRegisteredFromFactory>().ToList();
            }

            return obj;
        }

        [Benchmark(Description = "v1.BoDi_Concurrent_Dictionary_And_Lazy")]
        public object Version_1_BoDi_Concurrent_Dictionary_And_Lazy()
        {
            IEnumerable<IAllRegisteredFromFactory> obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = Container1Concurrent_Dictionary_And_Lazy.ResolveAll<IAllRegisteredFromFactory>();
            }

            return obj;
        }

        [Benchmark(Description = "Current")]
        public object CurrentVersion()
        {
            List<IAllRegisteredFromFactory> obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                // current returns a yet unresolved IEnumerable, so we need to force resolution
                obj = ContainerCurrent.ResolveAll<IAllRegisteredFromFactory>().ToList();
            }

            return obj;
        }
    }
}