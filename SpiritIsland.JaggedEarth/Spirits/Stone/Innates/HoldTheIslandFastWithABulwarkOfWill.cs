namespace SpiritIsland.JaggedEarth;

[InnatePower( "Hold the Island Fast with Bulwark of Will" ), Fast, Yourself]
class HoldTheIslandFastWithABulwarkOfWill {

	[InnateOption("2 earth","When blight is added to one of your lands, you may pay 2 Energy per blight to take it from the box instead of the Blight Card.")]
	static public Task Option1( SelfCtx ctx ) {
		ctx.GameState.Tokens[BlightCard.Space].Init( new PayEnergyToTakeFromBox(ctx,2), 1 );
		return Task.CompletedTask;
	}

	[InnateOption("4 earth","The cost is 1 Energy instead of 2")]
	static public Task Option2( SelfCtx ctx ) {
		ctx.GameState.Tokens[BlightCard.Space].Init( new PayEnergyToTakeFromBox(ctx,1), 1 );
		return Task.CompletedTask;
	}

	[InnateOption("6 earth,1 plant","When an Event or Blight card directly destroys presence (yours or others'), you may prevent any number of presence from being destroyed by paying 1 Energy each.",1)]
	static public Task Option3( SelfCtx ctx ) {
		ctx.GameState.AddIslandMod( new StopPresenceDestructionFromBlightOrEvents(ctx.Self)); // needs removed
		return Task.CompletedTask;
	}

}

/// <summary>
/// Add this to Blight Card to stop blight from coming off of it.
/// </summary>
class PayEnergyToTakeFromBox 
	: BaseModEntity
	, IModifyRemovingTokenAsync
	, IEndWhenTimePasses
{
	readonly Spirit _spirit;
	readonly int _cost;

	public PayEnergyToTakeFromBox( SelfCtx ctx, int cost ) {
		_spirit = ctx.Self;
		_cost = cost;
	}

	public async Task ModifyRemovingAsync( RemovingTokenArgs args ) {
		if( args.Token != Token.Blight || 0 == args.Count ) return;
		
		var cause = BlightToken.ForThisAction.BlightFromCardTrigger;
		if( _spirit.Presence.IsOn( cause.To ) // was taken from space with presence
		) {
			bool takeFromBoxInstead = _cost <= _spirit.Energy
				&& await _spirit.UserSelectsFirstText( $"New Blight on {cause.To.Space.Label}, take from:", $"Bag (for {_cost})", "card" );
			if(takeFromBoxInstead) {
				_spirit.Energy -= _cost;
				args.Count = 0;
			}
		}

	}

}

class StopPresenceDestructionFromBlightOrEvents : BaseModEntity, IModifyRemovingTokenAsync, IEndWhenTimePasses {
	readonly Spirit _spirit;

	public StopPresenceDestructionFromBlightOrEvents( Spirit spirit ) {
		_spirit = spirit;
	}

	public async Task ModifyRemovingAsync( RemovingTokenArgs args ) {
		if( args.Token.Class.Category == TokenCategory.Presence
			&& 1 <= _spirit.Energy
			&& await _spirit.UserSelectsFirstText( "Blight Destroying Presence", "Pay 1 energy to save", "Pass" )
		){
			_spirit.Energy--;
			--args.Count;
		}
	}

}