namespace SpiritIsland.Horizons;

public class SwelteringExhaustion {

	public const string Name = "Sweltering Exhaustion";

	[SpiritCard(Name, 2, Element.Fire, Element.Air), Fast, FromSacredSite(2)]
	[Instructions("Skip up to 1 Ravage/Build Action"), Artist(Artists.LucasDurham)]
	static public Task ActAsync(TargetSpaceCtx ctx) {
		// Skip up to 1 Ravage/Build Action
		ctx.Space.Init(new SkipRavageOrBuild(Name,ctx.Self),1);
		return Task.CompletedTask;
	}

}

public class SkipRavageOrBuild(string label, Spirit spirit)
	: BaseModEntity()
	, IEndWhenTimePasses
	, ISkipRavages
	, ISkipBuilds {

	readonly Spirit _spirit = spirit;

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Heavy;

	public string Text { get; } = label;

	Task<bool> ISkipRavages.Skip(Space space) => MakeDecision(space, "Ravage");

	Task<bool> ISkipBuilds.Skip(Space space) => MakeDecision(space, "Building ");

	async Task<bool> MakeDecision(Space space, string stoppableAction) {
		if( !await _spirit.UserSelectsFirstText($"{Text} - Stop {stoppableAction} on {space.SpaceSpec.Label}?", $"Yes, Stop {space.SpaceSpec.Label} {stoppableAction}.", "No thank you.") )
			return false;

		space.Adjust(this, -1);
		return true;
	}

}