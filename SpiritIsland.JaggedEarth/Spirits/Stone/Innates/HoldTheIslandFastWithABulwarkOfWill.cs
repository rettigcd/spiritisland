using System;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	// This isn't really an Action, it is more of a special ability.
	// User shouldn't have to click-on-it to activate it.


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
			_ = new StopPresenceDestructionFromBlight( ctx );
			return Task.CompletedTask;
		}

	}

	class PayEnergyToTakeFromCard {
		readonly Spirit spirit;
		readonly int cost;
		readonly Func<TokenBinding, int, Task<int>> oldBehavior;
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
		async Task<int> AddBlight( TokenBinding blight, int delta ) {
			blight.Count = blight + delta;
			
			if(delta > 0 
				&& cost <= spirit.Energy
				&& await spirit.UserSelectsFirstText( $"Take Blight From:", "Bag (for ${cost})", "card" )
			) {
				spirit.Energy -= cost;
				return 0;
			}
			return delta;
		}

	}

	class StopPresenceDestructionFromBlight {
		readonly Spirit spirit;
		readonly Func<Spirit, Task> oldBehavior;
		public StopPresenceDestructionFromBlight( TargetSpiritCtx ctx ) {
			this.spirit = ctx.Self;
			this.oldBehavior = ctx.GameState.Destroy1PresenceFromBlight;
			ctx.GameState.Destroy1PresenceFromBlight = this.AddBlight;
			ctx.GameState.TimePasses_ThisRound.Push( Restore );
		}

		Task Restore( GameState gs ) {
			gs.Destroy1PresenceFromBlight = oldBehavior;
			return Task.CompletedTask;
		}

		/// <returns># of blight to remove from card</returns>
		async Task AddBlight( Spirit other ) {

			if( 1 <= spirit.Energy
				&& await spirit.UserSelectsFirstText( "Blight Destroying Presence","Pay 1 energy to save","Pass")
			)
				spirit.Energy --;
			else 
				await oldBehavior(other);
		}

	}


}
