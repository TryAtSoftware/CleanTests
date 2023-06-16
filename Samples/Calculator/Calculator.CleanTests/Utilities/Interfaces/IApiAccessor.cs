namespace Calculator.CleanTests.Utilities.Interfaces;

public interface IApiAccessor
{
    Uri Host { get; }
    HttpClient HttpClient { get; }
    IServiceProvider Services { get; }
}