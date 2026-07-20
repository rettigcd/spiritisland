namespace SpiritIsland;

/// <summary>
/// Implements the "When Dahan would be Destroyed, Destroy N Fewer"
/// </summary>
public class SaveDahan : BaseModEntity, IEndWhenTimePasses, IModifyRemovingToken, ISerializableSpaceEntity {

	/// <summary>
	/// Builds an Action that stops future dahan from being destroyed.
	/// </summary>
	public static Action<TargetSpaceCtx> DestroyFewer(int maxPerAction, bool repeat)
		=> ctx => ctx.Space.Adjust(new SaveDahan(maxPerAction, repeat), 1);

	SaveDahan(int maxPerAction, bool repeat) : base() {
		_repeat = repeat;
		_maxPerAction = maxPerAction;
	}

	Task IModifyRemovingToken.ModifyRemovingAsync(RemovingTokenArgs args) {
		bool shouldReduce = args.Token.Class == Human.Dahan // Dahan
			&& args.Reason == RemoveReason.Destroyed; // Destroyed

		if( shouldReduce ) {
			var scope = ActionScope.Current;
			Space space = args.From;
			// Non-repeating: only save Dahan during the first action that triggers us, then remove self.
			if( !_repeat && !scope.ContainsKey(_key) )
				scope.AtEndOfThisAction( _ => space.Adjust(this, -1) );

			// If we haven't saved our allotment
			int previous = scope.SafeGet(_key, 0);
			if( previous < _maxPerAction ) {  // remaining adjustments for this action
											  // save some dahan
				int adjCount = Math.Min(_maxPerAction - previous, args.Count);
				args.Count -= adjCount;
				scope.SafeSet(_key, previous + adjCount);
				// restore to full health
				space.Humans(adjCount, args.Token.AsHuman()).Heal();
			} else {
				// make sure our already-saved dahan stay saved
				int maxWeCanDestroy = space.Dahan.CountAll - _maxPerAction;
				int countToReSave = args.Count - maxWeCanDestroy;
				if( 0 < countToReSave ) {
					args.Count -= countToReSave;
					space.Humans(countToReSave, args.Token.AsHuman()).Heal();
				}
			}
		}
		return Task.CompletedTask;
	}

	#region readonly
	readonly bool _repeat;
	readonly int _maxPerAction;
	readonly string _key = "SaveDahan-" + Guid.NewGuid().ToString();
	#endregion

	// _key isn't captured - it's only used to look up state in the current ActionScope, which is
	// action-scoped (not persisted), so a freshly-generated GUID on FromJson is equivalent.
	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, _maxPerAction, _repeat );

	const string Tag = "SaveDahan";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new SaveDahan( json[1]!.GetValue<int>(), json[2]!.GetValue<bool>() ) );

}
