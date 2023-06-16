namespace Calculator.CleanTests;

using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Calculator.CleanTests.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Randomizer.Core.Helpers;
using Xunit.Abstractions;

public class TriangleCleanTest : ApiCleanTest
{
    public TriangleCleanTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [CleanFact]
    [WithRequirements(InitializationCategories.ApiSpecification)]
    public async Task PerimeterShouldBeCalculatedSuccessfully()
    {
        var side1 = RandomizationHelper.RandomInteger(10, 1000);
        var side2 = RandomizationHelper.RandomInteger(10, 1000);
        var side3 = RandomizationHelper.RandomInteger(10, 1000);
        var arguments = new Dictionary<string, object> { { "side1", side1 }, { "side2", side2 }, { "side3", side3 } };

        var url = this.ApiSpecification.TrianglePerimeter.ConstructUrl(arguments);
        var inputModel = this.ApiSpecification.TrianglePerimeter.ConstructInputModel(arguments);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(this.ApiAccessor.Host, url));
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(inputModel), Encoding.UTF8, MediaTypeNames.Application.Json);

        var response = await this.ApiAccessor.HttpClient.SendAsync(httpRequest, this.GetCancellationToken());
        response.EnsureSuccessStatusCode();

        var responseModel = await response.Content.ReadFromJsonAsync(this.ApiSpecification.TrianglePerimeter.ResponseType, cancellationToken: this.GetCancellationToken());
        this.Equalizer.AssertEquality(side1 + side2 + side3, responseModel);
    }
}