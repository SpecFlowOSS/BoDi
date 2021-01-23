using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BODi.Performance.Tests.Benchmarks;

namespace BoDi.Performance.Tests.Benchmarks
{
    public class ResolveMultiThreadedMultipleContainer : MultipleContainerBenchmarkBase
    {
        [Params(2, 4, 8, 16)]
        public int ThreadCount { get; set; }

        [Params(2048)]
        public int Loops { get; set; }

        [Benchmark(Description = "Current")]
        public void CurrentVersion()
        {
            void Resolve(object _)
            {
                for (int i = 0; i < Loops / ThreadCount; i++)
                {
                    _ = ContainerCurrentLevel2.ResolveAll<TypeRegisteredLevel4>();
                    _ = ContainerCurrentLevel3.ResolveAll<TypeRegisteredLevel3>().ToList();
                    _ = ContainerCurrentLevel3.Resolve<TypeRegisteredLevel4>();
                    _ = ContainerCurrentLevel4.ResolveAll<TypeRegisteredLevel4>();
                    _ = ContainerCurrentLevel2.ResolveAll<TypeRegisteredLevel3>().ToList();
                    _ = ContainerCurrentLevel1.Resolve<TypeRegisteredLevel2>();
                    _ = ContainerCurrentLevel2.Resolve<TypeRegisteredLevel2>();
                    _ = ContainerCurrentLevel1.ResolveAll<TypeRegisteredLevel3>().ToList();
                    _ = ContainerCurrentLevel1.Resolve<TypeRegisteredLevel1>();
                    _ = ContainerCurrentLevel1.Resolve<TypeRegisteredLevel4>();
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
