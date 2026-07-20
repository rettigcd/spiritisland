namespace SpiritIsland;


/// <summary>
/// Can be added to GameState and runs after each invader phase
/// </summary>
public interface IRunAfterInvaderPhase {
	/// <returns>If object should be Removed from the TimePasses</returns>
	Task AfterInvaderPhase( GameState gameState );
	/// <summary> Indicates if action should be removed after running once. </summary>
	bool RemoveAfterRun { get; }

	/// <summary>
	/// Same shape as IRunBeforeInvaderPhase.ToJson() (docs/GameSerialization-Roadmap.md section 10) -
	/// added directly to the interface so PostInvaderPhaseActionRegistry/ActionList&lt;IRunAfterInvaderPhase&gt;
	/// can call it generically. TriggerAfterNoRavageOrBuild is currently the only implementer.
	/// </summary>
	JsonArray ToJson( ISerializationContext ctx );
}
