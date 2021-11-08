using System;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower( "Hold the Island Fast with Bulwark of Will" ), Fast, Yourself]
	class HoldTheIslandFastWithABulwarkOfWill {

		[InnateOption("2 earth","When blight is added to one of your lands, you may pay 2 Energy per blight to take it from the box instead of the Blight Card.")]
		static public Task Option1(TargetSpiritCtx ctx ) {
			_ = new PayEnergyToTakeFromCard(ctx,2);
			return Task.CompletedTask;
		}

		[InnateOption("4 earth","The cost is 1 Energy instead of 2")]
		static public Task Option2( TargetSpiritCtx ctx ) {
			_ = new PayEnergyToTakeFromCard( ctx, 1 );
			return Task.CompletedTask;
		}

		[InnateOption("6 earth,1 plant","When an Event or Blight card directly destroys presence (yours or others'), you may prevent any number of presence from being destroyed by paying 1 Energy each.",1)]
		static public Task Option3( TargetSpiritCtx ctx ) {
			_ = new StopPresenceDestructionFromBlightOrEvents( ctx );
			return Task.CompletedTask;
		}

	}

	class PayEnergyToTakeFromCard {

		readonly Spirit spirit;
		readonly int cost;
		readonly Func<TokenCountDictionary, int, Task<int>> oldBehavior;
		public PayEnergyToTakeFromCard( TargetSpiritCtx ctx, int cost ) {
			this.spirit = ctx.Self;
			this.cost = cost;
			this.oldBehavior = ctx.GameState.AddBlightBehavior;
			ctx.GameState.AddBlightBehavior = this.AddBlight;
			ctx.GameState.TimePasses_ThisRound.Push( Restore );
		}

		Task Restore(GameState gs ) {
			gs.AddBlightBehavior = oldBehavior; 
			return Task.CompletedTask;
		}

		/// <returns># of blight to remove from card</returns>
		async Task<int> AddBlight( TokenCountDictionary tokens, int delta ) {
			tokens.Blight.Count += delta;
			
			if(delta > 0 
				&& spirit.Presence.IsOn( tokens.Space )
				&& cost <= spirit.Energy
				&& await spirit.UserSelectsFirstText( $"New Blight on {tokens.Space.Label}, take from:", $"Bag (for {cost})", "card" )
			) {
				spirit.Energy -= cost;
				return 0;
			}
			return delta;
		}

	}

	class StopPresenceDestructionFromBlightOrEvents {
		readonly Spirit spirit;
		readonly Func<Spirit,GameState, Task> oldBehavior;
		public StopPresenceDestructionFromBlightOrEvents( TargetSpiritCtx ctx ) {
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
		async Task DestroyPresenceDirectlyFromBlight( Spirit other, GameState gs ) {

			if( 1 <= spirit.Energy
				&& await spirit.UserSelectsFirstText( "Blight Destroying Presence","Pay 1 energy to save","Pass")
			)
				spirit.Energy --;
			else 
				await oldBehavior(other,gs);
		}

	}


}
