namespace Calculator.CleanTests.Utilities;

using Calculator.CleanTests.Utilities.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;

public class ApiAccessor<TEntryPoint> : IApiAccessor
    where TEntryPoint : class
{
    private readonly WebApplicationFactory<TEntryPoint> _webApplicationFactory;

    public ApiAccessor(Uri host, WebApplicationFactory<TEntryPoint> webApplicationFactory)
    {
        this.Host = host ?? throw new ArgumentNullException(nameof(host));
        this._webApplicationFactory = webApplicationFactory ?? throw new ArgumentNullException(nameof(webApplicationFactory));
        this.HttpClient = this._webApplicationFactory.CreateClient();
    }

    public Uri Host { get; }
    public HttpClient HttpClient { get; }
    public IServiceProvider Services => this._webApplicationFactory.Services;
}