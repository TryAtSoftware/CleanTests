using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using TryAtSoftware.CleanTests.Benchmark;

// BenchmarkRunner.Run<CombinatorialMachineBenchmark>();
BenchmarkRunner.Run<DependenciesManagerBenchmark>(/*ManualConfig.Create(DefaultConfig.Instance).AddDiagnoser(EventPipeProfiler.Default)*/);
