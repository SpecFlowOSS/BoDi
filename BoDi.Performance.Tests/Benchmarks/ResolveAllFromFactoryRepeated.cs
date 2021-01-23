using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class ResolveAllFromFactoryRepeated : SingleContainerBenchmarkBase
    {
        [Params(10)]
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

        [Benchmark(Description = "v1.Next-Flawed")]
        public object Version_1_Next_Flawed()
        {
            IEnumerable<IAllRegisteredFromFactory> obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = Container1NextFlawed.ResolveAll<IAllRegisteredFromFactory>();
            }

            return obj;
        }

        [Benchmark(Description = "Current")]
        public object CurrentVersion()
        {
            IEnumerable<IAllRegisteredFromFactory> obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = ContainerCurrent.ResolveAll<IAllRegisteredFromFactory>();
            }

            return obj;
        }
    }
}