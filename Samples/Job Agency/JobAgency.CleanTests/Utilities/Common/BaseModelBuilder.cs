namespace JobAgency.CleanTests.Utilities.Common;

using TryAtSoftware.Randomizer.Core.Interfaces;

public abstract class BaseModelBuilder<TEntity, TOptions, TSetup> : IModelBuilder<TEntity, TOptions>
{
    public async Task<TEntity> BuildInstanceAsync(TOptions options, CancellationToken cancellationToken)
    {
        var setup = await this.ExecuteInitializationAsync(options, cancellationToken);
        
        var randomizer = this.ConstructRandomizer(options, setup);
        return randomizer.PrepareRandomValue();
    }

    protected abstract Task<TSetup> ExecuteInitializationAsync(TOptions options, CancellationToken cancellationToken);

    protected abstract IRandomizer<TEntity> ConstructRandomizer(TOptions options, TSetup setup);
}

public abstract class BaseModelBuilder<TEntity, TOptions> : BaseModelBuilder<TEntity, TOptions, Nothing>
{
    protected sealed override Task<Nothing> ExecuteInitializationAsync(TOptions options, CancellationToken cancellationToken) => Task.FromResult(Nothing.Instance);

    protected sealed override IRandomizer<TEntity> ConstructRandomizer(TOptions options, Nothing setup) => this.ConstructRandomizer(options);
    protected abstract IRandomizer<TEntity> ConstructRandomizer(TOptions options);
}

public abstract class BaseModelBuilder<TEntity> : BaseModelBuilder<TEntity, Nothing>
{
    protected sealed override IRandomizer<TEntity> ConstructRandomizer(Nothing options) => this.ConstructRandomizer();
    protected abstract IRandomizer<TEntity> ConstructRandomizer();
}