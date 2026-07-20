namespace SpiritIsland;

/// <summary>
/// Can be added to GameState and runs before each invader phase. Registered directly on
/// GameState (PreInvaderPhaseActionList), never placed on a Space - not an ISpaceEntity.
/// </summary>
public interface IRunBeforeInvaderPhase {
	/// <returns>If object should be Removed from the TimePasses</returns>
	Task BeforeInvaderPhase( GameState gameState );
	/// <summary> Indicates if action should be removed after running once. </summary>
	bool RemoveAfterRun { get; }

	/// <summary>
	/// Same shape as IFearCard.ToJson() (section 6) - added directly to the interface so
	/// PreInvaderPhaseActionRegistry/ActionList&lt;IRunBeforeInvaderPhase&gt; can call it generically.
	/// BlightCard already has a compatible virtual ToJson(ISerializationContext) (section 7), so the
	/// 7 self-registering blight cards satisfy this for free - though PreInvaderPhaseActionRegistry has
	/// no reader for their tags yet (see its own doc comment), so deserializing one still throws.
	/// </summary>
	JsonArray ToJson( ISerializationContext ctx );
}
