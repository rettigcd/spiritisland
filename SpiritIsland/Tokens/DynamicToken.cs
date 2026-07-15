namespace SpiritIsland;

/// <summary>
/// Island Mod that adds/removes a token based on some dynamic value - THIS ROUND ONLY
/// </summary>
public abstract class DynamicToken(TokenVariety dynamicToken)
	: BaseModEntity
	, IHandleTokenAdded, IHandleTokenRemoved
	, ICleanupSpaceWhenTimePasses
	, IEndWhenTimePasses
{

	protected abstract int GetCount(Space space);

	Task IHandleTokenAdded.HandleTokenAddedAsync(Space to, ITokenAddedArgs args) => Update(args.To);

	Task IHandleTokenRemoved.HandleTokenRemovedAsync(ITokenRemovedArgs args) => Update(args.From);

	#region private

	void ICleanupSpaceWhenTimePasses.CleanupSpace(Space space) => SetTokenCount(space, 0);

	Task Update(ILocation location) {
		if( location is Space space )
			SetTokenCount(space, GetCount(space));
		return Task.CompletedTask;
	}

	void SetTokenCount(Space space, int count) => space.Init(dynamicToken, count);

	#endregion private

}