namespace SpiritIsland.JaggedEarth;

[InnatePower( "Hold the Island Fast with Bulwark of Will" ), Fast, Yourself]
class HoldTheIslandFastWithABulwarkOfWill {

	[InnateTier("2 earth","When blight is added to one of your lands, you may pay 2 Energy per blight to take it from the box instead of the Blight Card.")]
	static public Task Option1( Spirit self ) {
		GameState.Current.Tokens[BlightCard.Space].Init( new PayEnergyToTakeFromBox(self,2), 1 );
		return Task.CompletedTask;
	}

	[InnateTier("4 earth","The cost is 1 Energy instead of 2")]
	static public Task Option2( Spirit self ) {
		GameState.Current.Tokens[BlightCard.Space].Init( new PayEnergyToTakeFromBox(self,1), 1 );
		return Task.CompletedTask;
	}

	[InnateTier("6 earth,1 plant","When an Event or Blight card directly destroys presence (yours or others'), you may prevent any number of presence from being destroyed by paying 1 Energy each.",1)]
	static public Task Option3( Spirit self ) {
		GameState.Current.AddIslandMod( new StopPresenceDestructionFromBlightOrEvents(self)); // needs removed
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

	public PayEnergyToTakeFromBox( Spirit self, int cost ) {
		_spirit = self;
		_cost = cost;
	}

	async Task IModifyRemovingTokenAsync.ModifyRemovingAsync( RemovingTokenArgs args ) {
		if( args.Token != Token.Blight || 0 == args.Count ) return;
		
		var cause = BlightToken.ForThisAction.BlightFromCardTrigger;
		if( _spirit.Presence.IsOn( cause.To ) // was taken from space with presence
		) {
			bool takeFromBoxInstead = _cost <= _spirit.Energy
				&& await _spirit.UserSelectsFirstText( $"New Blight on {cause.To.Label}, take from:", $"Bag (for {_cost})", "card" );
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

	async Task IModifyRemovingTokenAsync.ModifyRemovingAsync( RemovingTokenArgs args ) {
		if( args.Token.HasTag(TokenCategory.Presence)
			&& 1 <= _spirit.Energy
			&& await _spirit.UserSelectsFirstText( "Blight Destroying Presence", "Pay 1 energy to save", "Pass" )
		){
			_spirit.Energy--;
			--args.Count;
		}
	}

}