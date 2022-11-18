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

		public PreparedTokensOnSpaceTracker(TargetSpaceCtx ctx) { 
			this.ctx = ctx;
			tokenSummary = ctx.Tokens.Summary;

			elementToken = new ElementToken();
			ctx.Tokens.Init(elementToken,3);

			handlerKeys = new Guid[2];
			// !!! It seems like adding / removing presence tokens should trigger this also, but I don't think it triggers the Token Added/Removed event.
			handlerKeys[0] = ctx.GameState.Tokens.TokenAdded.ForGame.Add( Track );
			handlerKeys[1] = ctx.GameState.Tokens.TokenRemoved.ForGame.Add( Track );
		}

		Task Track( ITokenAddedArgs x ) => Check(x.Space.Space, x.ActionId);

		Task Track( ITokenRemovedArgs x ) => Check(x.Space, x.ActionId );

		async Task Check( Space space, Guid currentActionId ) {
			if(ctx.Tokens[elementToken] == 0			// already complete
				|| space != ctx.Space					// wrong space
				|| appliedActionsIds.Contains( currentActionId ) // already did this action 
				|| tokenSummary == ctx.Tokens.Summary 	// no change in tokens
			) return;

			if(currentActionId == default)
				throw new InvalidOperationException("Can't use default guids as actionids");

			appliedActionsIds.Add( currentActionId ); // limit to 1 change per action
			tokenSummary = ctx.Tokens.Summary;

			ctx.Tokens.Adjust(elementToken,-1);

			// !!! This web page states SMOA shouldn't get the element until after the Action completes.
			// https://boardgamegeek.com/thread/2399380/shifting-memory-observe-ever-changing-world
			var smoa = (ShiftingMemoryOfAges)ctx.Self;
			await smoa.PrepareElement(space.Label);
			if(ctx.Tokens[elementToken] == 0)
				ctx.GameState.TimePasses_ThisRound.Push( StopWatchingSpace );
		}

		Task StopWatchingSpace( GameState gs ) {
			gs.Tokens.TokenAdded.ForGame.Remove( handlerKeys[0] );
			gs.Tokens.TokenRemoved.ForGame.Remove( handlerKeys[1] );
			return Task.CompletedTask;
		}

		readonly TargetSpaceCtx ctx;
		readonly Guid[] handlerKeys;
		readonly ElementToken elementToken;
		readonly HashSet<Guid> appliedActionsIds = new HashSet<Guid>();
		string tokenSummary;

	}

	public class ElementToken : Token { // public for testing
		static int _total = 0;
		readonly int _index;
		public ElementToken() { _index = _total++; }
		public TokenClass Class => TokenType.Element;
		public string Text => "Element";
		public override string ToString() => $"AnyElement({_index})";
	}

}