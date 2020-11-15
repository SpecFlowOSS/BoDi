using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace BoDi.Performance.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, GetGlobalConfig());
        }

        static IConfig GetGlobalConfig()
#if DEBUG
            => new DebugInProcessConfig();
#else
            => DefaultConfig.Instance;
#endif
    }
}
