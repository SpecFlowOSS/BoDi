using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BODi.Performance.Tests.Benchmarks;

namespace BoDi.Performance.Tests.Benchmarks
{
    public class ResolveMultiThreadedSingleContainer : SingleContainerBenchmarkBase
    {
        [Params(2, 4, 8, 16)]
        public int ThreadCount { get; set; }

        [Params(2048)]
        public int Loops { get; set; }

        [Benchmark(Description = "v1.Next-Flawed")]
        public void Version_1_Next_Flawed()
        {
            void Resolve(object _)
            {
                for (int i = 0; i < Loops / ThreadCount; i++)
                {
                    _ = Container1NextFlawed.ResolveAll<IAllRegisteredFromFactory>().ToList();
                    _ = Container1NextFlawed.ResolveAll<IAllRegisteredFromType>().ToList();
                    _ = Container1NextFlawed.Resolve<FactoryRegistered>();
                    _ = Container1NextFlawed.Resolve<TypeRegistered>();
                }
            }
            StartAndJoin(Resolve);
        }

        [Benchmark(Description = "Current")]
        public void CurrentVersion()
        {
            void Resolve(object _)
            {
                for (int i = 0; i < Loops / ThreadCount; i++)
                {
                    _ = ContainerCurrent.ResolveAll<IAllRegisteredFromFactory>().ToList();
                    _ = ContainerCurrent.ResolveAll<IAllRegisteredFromType>().ToList();
                    _ = ContainerCurrent.Resolve<FactoryRegistered>();
                    _ = ContainerCurrent.Resolve<TypeRegistered>();
                }
            }
            StartAndJoin(Resolve);
        }

        private void StartAndJoin(ParameterizedThreadStart parameterizedThreadStart)
        {
            var threads = Enumerable.Range(1, ThreadCount)
                .Select(_ => new Thread(parameterizedThreadStart))
                .ToList();
            foreach (var t in threads)
            {
                t.Start();
            }

            foreach (var t in threads)
            {
                t.Join();
            }
        }
    }
}
