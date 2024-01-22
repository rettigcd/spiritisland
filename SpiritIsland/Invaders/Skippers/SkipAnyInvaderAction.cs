namespace SpiritIsland;

public class SkipAnyInvaderAction( string label, Spirit spirit, Func<SpaceState, Task> alternativeAction = null ) 
	: BaseModEntity() // !!! could add this to SpaceState for simplicity
	, IEndWhenTimePasses
	, ISkipRavages
	, ISkipBuilds
	, ISkipExploreTo
{

	readonly Func<SpaceState, Task> _alternativeAction = alternativeAction;
	readonly Spirit _spirit = spirit;

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Heavy;

	public string Text { get; } = label;

	Task<bool> ISkipRavages.Skip( SpaceState space ) => MakeDecision( space, "Ravage" );

	Task<bool> ISkipBuilds.Skip( SpaceState space ) => MakeDecision( space, "Building " );

	Task<bool> ISkipExploreTo.Skip( SpaceState space ) => MakeDecision( space, "Explore" );

	async Task<bool> MakeDecision( SpaceState space, string stoppableAction ) {
		if( !await _spirit.UserSelectsFirstText($"{Text} - Stop {stoppableAction} on {space.Space.Label}?", $"Yes, Stop {space.Space.Label} {stoppableAction}.", "No thank you.") )
			return false;

		space.Adjust( this, -1 );
		if(_alternativeAction != null)
			await _alternativeAction( space );
		return true;
	}

}