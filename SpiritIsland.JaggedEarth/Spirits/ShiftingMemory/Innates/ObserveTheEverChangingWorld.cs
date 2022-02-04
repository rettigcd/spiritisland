namespace SpiritIsland.JaggedEarth;

[InnatePower(ObserveTheEverChangingWorld.Name), Fast, FromPresence(1)]
public class ObserveTheEverChangingWorld {

	public const string Name = "Observe the Ever-Changing World";

	[InnateOption("1 moon","Prepare 1 Element Marker")]
	static public Task Option1(TargetSpaceCtx ctx ) {
		var smoa = (ShiftingMemoryOfAges)ctx.Self;
		return smoa.PrepareElement(ObserveTheEverChangingWorld.Name);
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
			handlerKeys[0] = ctx.GameState.Tokens.TokenAdded.ForGame.Add( Track );
			handlerKeys[1] = ctx.GameState.Tokens.TokenRemoved.ForGame.Add( Track );
		}

		Task Track( ITokenAddedArgs x ) => Check(x.Space);

		Task Track( ITokenRemovedArgs x ) => Check(x.Space);

		async Task Check( Space space ) {
			if(remaining == 0							// already complete
				|| space != ctx.Space					// wrong space
				|| ctx.Self.CurrentActionId == actionId // already did this action
				|| tokenSummary == ctx.Tokens.Summary 	// no change in tokens
			) return;

			actionId = ctx.Self.CurrentActionId; // limit to 1 change per action
			tokenSummary = ctx.Tokens.Summary;

			var smoa = (ShiftingMemoryOfAges)ctx.Self;
			await smoa.PrepareElement(space.Label);
			if(--remaining <= 0)
				ctx.GameState.TimePasses_ThisRound.Push( StopWatchingSpace );
		}

		Task StopWatchingSpace( GameState gs ) {
			gs.Tokens.TokenAdded.ForGame.Remove( handlerKeys[0] );
			gs.Tokens.TokenRemoved.ForGame.Remove( handlerKeys[1] );
			gs.Tokens.TokenMoved.ForGame.Remove( handlerKeys[2] );
			return Task.CompletedTask;
		}
	}

}