namespace SpiritIsland.JaggedEarth;

[InnatePower( "Hold the Island Fast with Bulwark of Will" ), Fast, Yourself]
class HoldTheIslandFastWithABulwarkOfWill {

	[InnateOption("2 earth","When blight is added to one of your lands, you may pay 2 Energy per blight to take it from the box instead of the Blight Card.")]
	static public Task Option1( SelfCtx ctx ) {
		ctx.GameState.AddIslandMod( new PayEnergyToTakeFromCard(ctx,2) ); // needs removed
		return Task.CompletedTask;
	}

	[InnateOption("4 earth","The cost is 1 Energy instead of 2")]
	static public Task Option2( SelfCtx ctx ) {
		ctx.GameState.AddIslandMod( new PayEnergyToTakeFromCard(ctx,1) ); // needs removed
		return Task.CompletedTask;
	}

	[InnateOption("6 earth,1 plant","When an Event or Blight card directly destroys presence (yours or others'), you may prevent any number of presence from being destroyed by paying 1 Energy each.",1)]
	static public Task Option3( SelfCtx ctx ) {
		ctx.GameState.AddIslandMod( new StopPresenceDestructionFromBlightOrEvents(ctx.Self)); // needs removed
		return Task.CompletedTask;
	}

}

class PayEnergyToTakeFromCard 
	: BaseModEntity
	, IModifyAddingToken
	, IEndWhenTimePasses
{

	readonly Spirit _spirit;
	readonly int _cost;

	public PayEnergyToTakeFromCard( SelfCtx ctx, int cost ) {
		_spirit = ctx.Self;
		_cost = cost;
	}

	public void ModifyAdding( AddingTokenArgs args ) {
		if( args.Token == Token.Blight 
			&& 0 < args.Count 
			&& args.To.Has( _spirit.Token )
		)
			BlightToken.ForThisAction.CustomTakeFromBlightSouce = this.AddBlight;
	}

	async Task AddBlight( int delta, SpaceState space ) {
		bool takeFromBagInstead = 0 < delta
			&& space.Has( _spirit.Token )
			&& _cost <= _spirit.Energy
			&& await _spirit.UserSelectsFirstText( $"New Blight on {space.Space.Label}, take from:", $"Bag (for {_cost})", "card" );

		if(takeFromBagInstead)
			_spirit.Energy -= _cost;
		else
			await GameState.Current.TakeBlightFromCard( delta );
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