namespace SpiritIsland;

public class DahanSaver : SelfCleaningToken, IHandleRemovingToken {

	public static Action<TargetSpaceCtx> DestroyFewer( int maxPerAction, int maxActionCount )
		=> ctx => ctx.Tokens.Adjust( new DahanSaver( ctx.Tokens, maxPerAction, maxActionCount ), 1 );

	#region readonly
	readonly CountDictionary<ActionScope> byAction = new();
	readonly SpaceState space;
	readonly int maxActionCount;
	readonly int maxPerAction;
	#endregion

	DahanSaver( SpaceState space, int maxPerAction, int maxActionCount ) :base() {
		this.space = space;
		this.maxActionCount = maxActionCount;
		this.maxPerAction   = maxPerAction;
	}

	Task IHandleRemovingToken.ModifyRemoving( RemovingTokenArgs args ) { ReduceDestroyCount(args); return Task.CompletedTask; }
	void ReduceDestroyCount( RemovingTokenArgs args ) {

		bool shouldReduce = args.Token.Class == Human.Dahan // Dahan
			&& (args.Reason == RemoveReason.Destroyed) // Destroyed
			&& (byAction.Count < maxActionCount || byAction.ContainsKey( ActionScope.Current ));              // can effect more action OR already added

		if(shouldReduce) {
			// If we haven't saved our allotment
			int previous = byAction[ActionScope.Current];
			if(previous < maxPerAction) {  // // remaining adjustments for this action
				// save some dahan
				int adjustment = Math.Min( maxPerAction - previous, args.Count );
				args.Count -= adjustment;
				byAction[ActionScope.Current] += adjustment;
				// restore to full health
				var savedToken = (HumanToken)args.Token;
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
