namespace SpiritIsland;

public class DahanSaver {

	public static Action<TargetSpaceCtx> DestroyFewer( int maxPerAction, int maxActionCount ) {
		return ctx => {
			var saver = new DahanSaver( ctx.Tokens, maxPerAction, maxActionCount );
			ctx.GameState.RemovingHandler_RegisterForSpace( ctx.Tokens, saver.ReduceDestroyCount );
		};
	}

	#region readonly
	readonly CountDictionary<UnitOfWork> byAction = new();
	readonly SpaceState space;
	readonly int maxActionCount;
	readonly int maxPerAction;
	#endregion

	DahanSaver( SpaceState space, int maxPerAction, int maxActionCount ) {
		this.space = space;
		this.maxActionCount = maxActionCount;
		this.maxPerAction   = maxPerAction;
	}

	public void ReduceDestroyCount( RemovingTokenArgs args ) {

		bool shouldReduce = args.Token.Class == TokenType.Dahan                                         // Dahan
			&& (args.Reason == RemoveReason.Destroyed || args.Reason == RemoveReason.DestroyedInBattle) // Destroyed
			&& (byAction.Count < maxActionCount || byAction.ContainsKey( args.ActionId ));              // can effect more action OR already added

		if(shouldReduce) {
			// If we haven't saved our allotment
			int previous = byAction[args.ActionId];
			if(previous < maxPerAction) {  // // remaining adjustments for this action
				// save some dahan
				int adjustment = Math.Min( maxPerAction - previous, args.Count );
				args.Count -= adjustment;
				byAction[args.ActionId] += adjustment;
				// restore to full health
				var savedToken = (HealthToken)args.Token;
				space.Adjust( args.Token, -adjustment );
				space.Adjust( savedToken.Healthy, adjustment );
			} else {
				// make sure our already-saved dahan stay saved
				if(args.Count > space.Dahan.CountAll - maxPerAction)
					args.Count = space.Dahan.CountAll - maxPerAction;
			}
		}
	}

}
