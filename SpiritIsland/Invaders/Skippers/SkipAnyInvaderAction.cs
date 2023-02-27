namespace SpiritIsland;

public class SkipAnyInvaderAction : BaseModEntity
	, IEndWhenTimePasses
	, ISkipRavages
	, ISkipBuilds
	, ISkipExploreTo
{

	readonly Func<SpaceState, Task> _alternativeAction;
	readonly Spirit _spirit;

	public SkipAnyInvaderAction( string label, Spirit spirit, Func<SpaceState, Task> alternativeAction = null )
		:base() // rated this high because it can stop builds and ravages also, maybe it should be lower
	{
		Text = label;
		_spirit = spirit;
		_alternativeAction = alternativeAction;
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Heavy;

	public string Text {get;}

	Task<bool> ISkipRavages.Skip( SpaceState space ) => MakeDecision( space, "Ravage" );

	Task<bool> ISkipBuilds.Skip( SpaceState space ) => MakeDecision( space, "Building " );

	Task<bool> ISkipExploreTo.Skip( SpaceState space ) => MakeDecision( space, "Explore" );

	async Task<bool> MakeDecision( SpaceState space, string stoppableAction ) {
		if( !await _spirit.UserSelectsFirstText(Text,  $"Stop {stoppableAction} on {space.Space.Label}?", "No thank you.") )
			return false;

		space.Adjust( this, -1 );
		if(_alternativeAction != null)
			await _alternativeAction( space );
		return true;
	}

}