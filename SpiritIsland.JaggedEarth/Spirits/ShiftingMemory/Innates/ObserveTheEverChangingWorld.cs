using System;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.ShiftingMemory {
	[InnatePower("Observe the Ever-Changing World"), Fast, FromPresence(1)]
	public class ObserveTheEverChangingWorld {

		[InnateOption("1 moon","Prepare 1 Element Marker")]
		static public Task Option1(TargetSpaceCtx ctx ) {
			return ctx.Self.PrepareElement();
		}

		[InnateOption("2 moon,1 air","Instead, after each of the next three Actions that change which pieces are in atarget land, Prepare 1 Element Marker.")]
		static public Task Option2(TargetSpaceCtx ctx ) {
			_ = new PreparedTokensOnSpaceTracker(ctx);
			return Task.CompletedTask;
		}

		class PreparedTokensOnSpaceTracker {
			readonly TargetSpaceCtx ctx;
			Guid actionId;
			readonly Guid[] handlerKeys;
			int remaining = 3;
			string tokenSummary;
			public PreparedTokensOnSpaceTracker(TargetSpaceCtx ctx ) { 
				this.ctx = ctx;
				actionId = ctx.Self.CurrentActionId;
				tokenSummary = ctx.Tokens.Summary;

				handlerKeys = new Guid[3];
				handlerKeys[0] = ctx.GameState.Tokens.TokenAdded.ForEntireGame( Track );
				handlerKeys[1] = ctx.GameState.Tokens.TokenDestroyed.ForEntireGame( Track );
				handlerKeys[2] = ctx.GameState.Tokens.TokenMoved.ForEntireGame( Track );
			}

			Task Track( GameState gs, TokenAddedArgs x ) => Check(x.Space);

			Task Track( GameState gs, TokenDestroyedArgs x ) => Check(x.Space);

			async Task Track( GameState gs, TokenMovedArgs x ) {
				await Check(x.From);
				await Check(x.To);
			}

			async Task Check( Space space ) {
				if(remaining == 0							// already complete
					|| space != ctx.Space					// wrong space
					|| ctx.Self.CurrentActionId == actionId // already did this action
					|| tokenSummary == ctx.Tokens.Summary 	// no change in tokens
				) return;

				actionId = ctx.Self.CurrentActionId; // limit to 1 change per action
				tokenSummary = ctx.Tokens.Summary;

				await ctx.Self.PrepareElement();
				if(--remaining <= 0)
					ctx.GameState.TimePasses_ThisRound.Push( StopWatchingSpace );
			}

			Task StopWatchingSpace( GameState gs ) {
				gs.Tokens.TokenAdded.Remove( handlerKeys[0] );
				gs.Tokens.TokenDestroyed.Remove( handlerKeys[1] );
				gs.Tokens.TokenMoved.Remove( handlerKeys[2] );
				return Task.CompletedTask;
			}
		}

	}


}
