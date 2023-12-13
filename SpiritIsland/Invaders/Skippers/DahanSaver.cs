namespace SpiritIsland;

/// <summary>
/// Implements the "When Dahan would be Destroyed, Destroy N Fewer"
/// </summary>
public class DahanSaver : BaseModEntity, IEndWhenTimePasses, IModifyRemovingToken {

	/// <summary>
	/// Builds an Action that stops future dahan from being destroyed.
	/// </summary>
	public static Action<TargetSpaceCtx> DestroyFewer( int maxPerAction, int maxActionCount )
		=> ctx => ctx.Tokens.Adjust( new DahanSaver( ctx.Tokens, maxPerAction, maxActionCount ), 1 );

	DahanSaver( SpaceState space, int maxPerAction, int maxActionCount ) :base() {
		_space = space;
		_maxActionCount = maxActionCount;
		_maxPerAction   = maxPerAction;
	}

	void IModifyRemovingToken.ModifyRemoving( RemovingTokenArgs args ) {
		bool shouldReduce = args.Token.Class == Human.Dahan // Dahan
			&& (args.Reason == RemoveReason.Destroyed) // Destroyed
			&& (_byAction.Count < _maxActionCount || _byAction.ContainsKey( ActionScope.Current )); // can effect more action OR already added

		if(shouldReduce) {
			// If we haven't saved our allotment
			int previous = _byAction[ActionScope.Current];
			if(previous < _maxPerAction) {  // remaining adjustments for this action
				// save some dahan
				int adjustment = Math.Min( _maxPerAction - previous, args.Count );
				args.Count -= adjustment;
				_byAction[ActionScope.Current] += adjustment;
				// restore to full health
				_space.Adjust( args.Token, -adjustment );
				_space.Adjust( args.Token.AsHuman().Healthy, adjustment );
			} else {
				// make sure our already-saved dahan stay saved
				if(args.Count > _space.Dahan.CountAll - _maxPerAction)
					args.Count = _space.Dahan.CountAll - _maxPerAction;
			}
		}

	}

	#region readonly
	readonly CountDictionary<ActionScope> _byAction = new();
	readonly SpaceState _space;
	readonly int _maxActionCount;
	readonly int _maxPerAction;
	#endregion

}
