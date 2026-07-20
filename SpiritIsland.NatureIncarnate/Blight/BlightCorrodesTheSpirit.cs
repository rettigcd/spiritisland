namespace SpiritIsland.NatureIncarnate;

public class BlightCorrodesTheSpirit : BlightCard, IRunBeforeInvaderPhase {

	public BlightCorrodesTheSpirit()
		:base("Blight Corrodes the Spirit",
			"Each Invader Phase: On Each Board, Destroy 1 Presence in a land with Blight.",
			4
		)
	{}

	public override IActOn<GameState> Immediately => RunAtTheStartOfEachInvadorPhase(this);

	bool IRunBeforeInvaderPhase.RemoveAfterRun => false;

	Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState )
		=> DestroyAnySpiritPresence()
			.In().OneLandPerBoard().Which(Has.Blight)
			.ForEachBoard()
			.ActAsync( gameState );

	// !!! Also, this needs converted into a collaborative process so that each spirit makes its own destroy decision.
	static public SpaceAction DestroyAnySpiritPresence() => new SpaceAction( 
		$"Destroy 1 presence", 
		ctx => ctx.Space.SourceSelector
			// !!! If .AddGroup took Tags instead of ITokenClass, we could just flag it all with TokenCategory.SpiritPresence
			.UseQuota(new Quota().AddGroup(1,GameState.Current.Spirits.Select(s=>s.Presence).ToArray()))
			.DestroyN(ctx.Self,Present.Always)
	);

	[ModuleInitializer]
	internal static void RegisterSerialization() {
		BlightCardRegistry.Register( nameof( BlightCorrodesTheSpirit ), ( json, ctx ) => new BlightCorrodesTheSpirit() );
		// Registers `this` as the IRunBeforeInvaderPhase entry - resolve to the live GameState.BlightCard,
		// not a fresh instance. See docs/GameSerialization-Roadmap.md section 10.
		PreInvaderPhaseActionRegistry.Register( nameof( BlightCorrodesTheSpirit ), ( json, ctx ) => (IRunBeforeInvaderPhase)ctx.BlightCard );
	}

}