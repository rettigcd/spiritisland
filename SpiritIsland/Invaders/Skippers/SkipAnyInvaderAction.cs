namespace SpiritIsland;

public class SkipAnyInvaderAction : SkipBase, ISkipRavages, ISkipBuilds, ISkipExploreTo {

	readonly Func<GameState, SpaceState, Task> alternativeAction;
	readonly Spirit _spirit;

	public SkipAnyInvaderAction( string label, Spirit spirit, Func<GameState, SpaceState, Task> alternativeAction = null )
		:base(label,UsageCost.Heavy ) // rated this high because it can stop builds and ravages also, maybe it should be lower
	{
		_spirit = spirit;
		this.alternativeAction = alternativeAction;
	}

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
		if(alternativeAction != null)
			await alternativeAction( gameState, space );
		return true;
	}

}