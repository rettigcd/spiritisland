namespace SpiritIsland;

public class SkipAnyInvaderAction : SelfCleaningToken, ISkipRavages, ISkipBuilds, ISkipExploreTo {

	readonly Func<GameState, SpaceState, Task> _alternativeAction;
	readonly Spirit _spirit;

	public SkipAnyInvaderAction( string label, Spirit spirit, Func<GameState, SpaceState, Task> alternativeAction = null )
		:base() // rated this high because it can stop builds and ravages also, maybe it should be lower
	{
		Text = label;
		_spirit = spirit;
		_alternativeAction = alternativeAction;
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Heavy;

	public string Text {get;}

	Task<bool> ISkipRavages.Skip( GameState gameState, SpaceState space )
		=> MakeDecision( gameState, space, "Ravage" );

	Task<bool> ISkipBuilds.Skip( GameCtx gameCtx, SpaceState space, TokenClass buildClass )
		=> MakeDecision( gameCtx.GameState, space, "Building " + buildClass.Label );

	Task<bool> ISkipExploreTo.Skip( GameCtx gameCtx, SpaceState space )
		=> MakeDecision( gameCtx.GameState, space, "Explore" );

	async Task<bool> MakeDecision( GameState gameState, SpaceState space, string stoppableAction ) {
		if( !await _spirit.UserSelectsFirstText(Text,  $"Stop {stoppableAction} on {space.Space.Label}?", "No thank you.") )
			return false;

		space.Adjust( this, -1 );
		if(_alternativeAction != null)
			await _alternativeAction( gameState, space );
		return true;
	}

}