using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class ResolveFromFactoryRepeated : SingleContainerBenchmarkBase
    {
        [Params(10)]
        public int Repetitions { get; set; }


        [Benchmark(Baseline = true, Description = "v1.4")]
        public object Version_1_4()
        {
            FactoryRegistered obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = Container14.Resolve<FactoryRegistered>();
            }

            return obj;
        }

        [Benchmark(Description = "v1.Next-Flawed")]
        public object Version_1_Next_Flawed()
        {
            FactoryRegistered obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = Container1NextFlawed.Resolve<FactoryRegistered>();
            }

            return obj;
        }

        [Benchmark(Description = "Current")]
        public object CurrentVersion()
        {
            FactoryRegistered obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = ContainerCurrent.Resolve<FactoryRegistered>();
            }

            return obj;
        }
    }
}