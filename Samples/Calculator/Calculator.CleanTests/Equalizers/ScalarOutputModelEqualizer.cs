namespace Calculator.CleanTests.Equalizers;

using Calculator.API.OutputModels.V1;
using TryAtSoftware.Equalizer.Core.Profiles.Complex;

public class ScalarOutputModelEqualizer : ComplexEqualizationProfile<int, ScalarOutputModel>
{
    public ScalarOutputModelEqualizer()
    {
        this.Equalize(x => x, x => x.Result);
    }
}