using BenchmarkDotNet.Attributes;

namespace BODi.Performance.Tests.Benchmarks
{
    public class ResolveFromTypeRepeated : SingleContainerBenchmarkBase
    {
        [Params(10)]
        public int Repetitions { get; set; }


        [Benchmark(Baseline = true, Description = "v1.4")]
        public object Version_1_4()
        {
            TypeRegistered obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = ContainerCurrent.Resolve<TypeRegistered>();
            }

            return obj;
        }

        [Benchmark(Description = "v1.Next-Flawed")]
        public object Version_1_Next_Flawed()
        {
            TypeRegistered obj = null;
            for (int i = 0; i < Repetitions; i++)
            {
                obj = ContainerCurrent.Resolve<TypeRegistered>();
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