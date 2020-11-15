using System.Linq;
using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class ResolveAllFromType : BenchmarkBase
    {
        [Benchmark(Baseline = true, Description = "v1.4")]
        public object Version_1_4()
        {
            // v1.4 returned a yet unresolved IEnumerable, so we need to force resolution
            return Container14.ResolveAll<IAllRegisteredFromType>().ToList();
        }

        [Benchmark(Description = "Current")]
        public object CurrentVersion()
        {
            return ContainerCurrent.ResolveAll<IAllRegisteredFromType>();
        }
    }
}