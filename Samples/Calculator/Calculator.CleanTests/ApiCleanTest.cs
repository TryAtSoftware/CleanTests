namespace Calculator.CleanTests;

using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Calculator.CleanTests.Constants;
using Calculator.CleanTests.Equalizers;
using Calculator.CleanTests.Utilities;
using Calculator.CleanTests.Utilities.Interfaces;
using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Equalizer.Core;
using TryAtSoftware.Equalizer.Core.Interfaces;
using TryAtSoftware.Equalizer.Core.ProfileProviders;
using Xunit.Abstractions;

[WithRequirements(InitializationCategories.ApiProvider)]
public abstract class ApiCleanTest : CleanTest
{
    private int _apiAccessorId;

    protected ApiCleanTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        this.Equalizer = this.PrepareEqualizer();
    }
    
    private IApiProvider ApiProvider { get; set; }

    protected IApiAccessor ApiAccessor { get; private set; }
    protected IApiSpecification ApiSpecification => this.GetGlobalService<IApiSpecification>();
    protected IEqualizer Equalizer { get; }

    public override async Task InitializeAsync()
    {
        this.InitializeGlobalDependenciesProvider();
        
        this.ApiProvider = this.GetGlobalService<IApiProvider>();
        this._apiAccessorId = await this.ApiProvider.GetResourceIdAsync(Nothing.Instance, this.GetCancellationToken());
        this.TestOutputHelper.WriteLine("The API accessor id is: {0}", this._apiAccessorId);
        this.ApiAccessor = this.ApiProvider.GetApiAccessor(this._apiAccessorId);
        
        this.InitializeLocalDependenciesProvider();
    }

    public override async Task DisposeAsync()
    {
        await this.ApiProvider.ReleaseResourceAsync(this._apiAccessorId, this.GetCancellationToken());
        await base.DisposeAsync();
    }

    protected async Task<object?> ExecutePostRequest(IApiOperationDescriptor operationDescriptor, IDictionary<string, object> arguments)
    {
        var url = operationDescriptor.ConstructUrl(arguments);
        var inputModel = operationDescriptor.ConstructInputModel(arguments);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(this.ApiAccessor.Host, url));
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(inputModel), Encoding.UTF8, MediaTypeNames.Application.Json);

        var response = await this.ApiAccessor.HttpClient.SendAsync(httpRequest, this.GetCancellationToken());
        response.EnsureSuccessStatusCode();

        var responseModel = await response.Content.ReadFromJsonAsync(operationDescriptor.ResponseType, cancellationToken: this.GetCancellationToken());
        return responseModel;
    }

#pragma warning disable CA1822 // This may be changed in future
    protected CancellationToken GetCancellationToken() => CancellationToken.None;
#pragma warning restore CA1822

    private IEqualizer PrepareEqualizer()
    {
        var equalizer = new Equalizer();
        var profilesProvider = new DedicatedProfileProvider();
        profilesProvider.AddProfile(new ScalarOutputModelEqualizer());
        equalizer.AddProfileProvider(profilesProvider);

        return equalizer;
    }
}