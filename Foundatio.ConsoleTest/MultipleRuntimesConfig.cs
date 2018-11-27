using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

namespace Foundatio.ConsoleTest
{
    public class MultipleRuntimesConfig : ManualConfig
    {
        public MultipleRuntimesConfig()
        {
            Add(Job.Default
                    .With(CsProjClassicNetToolchain.Net46) // Span NOT supported by Runtime
                    .WithId(".NET 4.6"));

            /// !!! warning !!! NetCoreApp20 toolchain simply sets TargetFramework = netcoreapp2.0 in generated .csproj
            /// // so you need Visual Studio 2017 Preview 15.3 to be able to run it!
            Add(Job.Default
                    .With(CsProjCoreToolchain.NetCoreApp20) // Span SUPPORTED by Runtime
                    .WithId(".NET Core 2.0"));
        }
    }
}
