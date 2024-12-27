namespace SpiritIsland;

public class SkipAnyInvaderAction(string label, Spirit spirit, Func<Space, Task>? alternativeAction = null)
	: BaseModEntity() // !!! could add this to Space for simplicity
	, IEndWhenTimePasses
	, ISkipRavages
	, ISkipBuilds
	, ISkipExploreTo {

	readonly Func<Space, Task>? _alternativeAction = alternativeAction;
	readonly Spirit _spirit = spirit;

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Heavy;

	public string Text { get; } = label;

	Task<bool> ISkipRavages.Skip(Space space) => MakeDecision(space, "Ravage");

	Task<bool> ISkipBuilds.Skip(Space space) => MakeDecision(space, "Building ");

	Task<bool> ISkipExploreTo.Skip(Space space) => MakeDecision(space, "Explore");

	async Task<bool> MakeDecision(Space space, string stoppableAction) {
		if( !await _spirit.UserSelectsFirstText($"{Text} - Stop {stoppableAction} on {space.SpaceSpec.Label}?", $"Yes, Stop {space.SpaceSpec.Label} {stoppableAction}.", "No thank you.") )
			return false;

		space.Adjust(this, -1);
		if( _alternativeAction is not null )
			await _alternativeAction(space);
		return true;
	}

}