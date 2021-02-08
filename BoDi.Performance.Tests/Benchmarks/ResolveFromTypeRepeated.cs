using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class ResolveFromTypeRepeated : SingleContainerBenchmarkBase
    {
        [Params(100)]
        public int Repetitions { get; set; }


        [Benchmark(Baseline = true, Description = "v1.4")]
        public object Version_1_4()
        {
            TypeRegistered obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = Container14.Resolve<TypeRegistered>();
            }

            return obj;
        }

        [Benchmark(Description = "v1.BoDi_Concurrent_Dictionary_And_Lazy")]
        public object Version_1_BoDi_Concurrent_Dictionary_And_Lazy()
        {
            TypeRegistered obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = Container1Concurrent_Dictionary_And_Lazy.Resolve<TypeRegistered>();
            }

            return obj;
        }

        [Benchmark(Description = "Current")]
        public object CurrentVersion()
        {
            TypeRegistered obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = ContainerCurrent.Resolve<TypeRegistered>();
            }

            return obj;
        }
    }
}