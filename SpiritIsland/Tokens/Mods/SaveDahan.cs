namespace SpiritIsland;

/// <summary>
/// Implements the "When Dahan would be Destroyed, Destroy N Fewer"
/// </summary>
public class SaveDahan : BaseModEntity, IEndWhenTimePasses, IModifyRemovingToken {

	/// <summary>
	/// Builds an Action that stops future dahan from being destroyed.
	/// </summary>
	public static Action<TargetSpaceCtx> DestroyFewer(int maxPerAction, int maxActionCount)
		=> ctx => ctx.Space.Adjust(new SaveDahan(ctx.Space, maxPerAction, maxActionCount), 1);

	SaveDahan(Space space, int maxPerAction, int maxActionCount) : base() {
		_space = space;
		_maxActionCount = maxActionCount;
		_maxPerAction = maxPerAction;
	}

	void IModifyRemovingToken.ModifyRemoving(RemovingTokenArgs args) {
		bool shouldReduce = args.Token.Class == Human.Dahan // Dahan
			&& args.Reason == RemoveReason.Destroyed // Destroyed
			&& (_byAction.Count < _maxActionCount || _byAction.ContainsKey(ActionScope.Current)); // can effect more action OR already added

		if( shouldReduce ) {
			// If we haven't saved our allotment
			int previous = _byAction[ActionScope.Current];
			if( previous < _maxPerAction ) {  // remaining adjustments for this action
											  // save some dahan
				int adjCount = Math.Min(_maxPerAction - previous, args.Count);
				args.Count -= adjCount;
				_byAction[ActionScope.Current] += adjCount;
				// restore to full health
				_space.Humans(adjCount, args.Token.AsHuman()).Heal();
			} else {
				// make sure our already-saved dahan stay saved
				int maxWeCanDestroy = _space.Dahan.CountAll - _maxPerAction;
				int countToReSave = args.Count - maxWeCanDestroy;
				if( 0 < countToReSave ) {
					args.Count -= countToReSave;
					_space.Humans(countToReSave, args.Token.AsHuman()).Heal();
				}
			}
		}

	}

	#region readonly
	readonly CountDictionary<ActionScope> _byAction = [];
	readonly Space _space;
	readonly int _maxActionCount;
	readonly int _maxPerAction;
	#endregion

}
