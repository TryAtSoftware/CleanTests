namespace JobAgency.CleanTests.Utilities.Common;

public interface IModelBuilder<TEntity, in TOptions>
{
    Task<TEntity> BuildInstanceAsync(TOptions options, CancellationToken cancellationToken);
}