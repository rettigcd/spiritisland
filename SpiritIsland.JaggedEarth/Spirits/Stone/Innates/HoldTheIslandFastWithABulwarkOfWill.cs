namespace SpiritIsland.JaggedEarth;

[InnatePower( "Hold the Island Fast with Bulwark of Will" ), Fast, Yourself]
class HoldTheIslandFastWithABulwarkOfWill {

	[InnateOption("2 earth","When blight is added to one of your lands, you may pay 2 Energy per blight to take it from the box instead of the Blight Card.")]
	static public Task Option1( SelfCtx ctx ) {
		_ = new PayEnergyToTakeFromCard(ctx,2);
		return Task.CompletedTask;
	}

	[InnateOption("4 earth","The cost is 1 Energy instead of 2")]
	static public Task Option2( SelfCtx ctx ) {
		_ = new PayEnergyToTakeFromCard( ctx, 1 );
		return Task.CompletedTask;
	}

	[InnateOption("6 earth,1 plant","When an Event or Blight card directly destroys presence (yours or others'), you may prevent any number of presence from being destroyed by paying 1 Energy each.",1)]
	static public Task Option3( SelfCtx ctx ) {
		_ = new StopPresenceDestructionFromBlightOrEvents( ctx );
		return Task.CompletedTask;
	}

}

class PayEnergyToTakeFromCard {

	readonly Spirit spirit;
	readonly int cost;
	readonly Func<int, SpaceState, Task> oldBehavior;
	public PayEnergyToTakeFromCard( SelfCtx ctx, int cost ) {
		this.spirit = ctx.Self;
		this.cost = cost;
		this.oldBehavior = ctx.GameState.TakeFromBlightSouce;
		ctx.GameState.TakeFromBlightSouce = this.AddBlight;
		ctx.GameState.TimePasses_ThisRound.Push( Restore );
	}

	Task Restore(GameState gs ) {
		gs.TakeFromBlightSouce = oldBehavior; 
		return Task.CompletedTask;
	}

	/// <returns># of blight to remove from card</returns>
	async Task AddBlight( int delta, SpaceState space ) {
		bool takeFromBagInstead = 0 < delta
			&& spirit.Presence.IsOn( space )
			&& cost <= spirit.Energy
			&& await spirit.UserSelectsFirstText( $"New Blight on {space.Space.Label}, take from:", $"Bag (for {cost})", "card" );

		if( takeFromBagInstead )
			spirit.Energy -= cost;
		else 
			await oldBehavior( delta, space );
	}

}

class StopPresenceDestructionFromBlightOrEvents {
	readonly Spirit spirit;
	readonly Func<Spirit,GameState,UnitOfWork, Task> oldBehavior;
	public StopPresenceDestructionFromBlightOrEvents( SelfCtx ctx ) {
		this.spirit = ctx.Self;
		this.oldBehavior = ctx.GameState.Destroy1PresenceFromBlightCard;
		ctx.GameState.Destroy1PresenceFromBlightCard = this.DestroyPresenceDirectlyFromBlight;
		ctx.GameState.TimePasses_ThisRound.Push( Restore );
	}

	Task Restore( GameState gs ) {
		gs.Destroy1PresenceFromBlightCard = oldBehavior;
		return Task.CompletedTask;
	}

	/// <returns># of blight to remove from card</returns>
	async Task DestroyPresenceDirectlyFromBlight( Spirit other, GameState gs, UnitOfWork actionScope ) {
		if( 1 <= spirit.Energy
			&& await spirit.UserSelectsFirstText( "Blight Destroying Presence","Pay 1 energy to save","Pass")
		)
			spirit.Energy --;
		else 
			await oldBehavior(other, gs, actionScope);
	}

}