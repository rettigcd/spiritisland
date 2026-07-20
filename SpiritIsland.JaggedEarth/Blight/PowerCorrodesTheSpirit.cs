namespace SpiritIsland.JaggedEarth;

public class PowerCorrodesTheSpirit : BlightCard, IRunBeforeInvaderPhase {

	public PowerCorrodesTheSpirit():base("Power Corrodes the Spirit", "Each Invader Phase: Each Spirit Destroys 1 of their presence if they have 3 or more Power Cards in play, or have a Power Card in play costing 4 or more (printed) Energy.", 4) {}

	public override IActOn<GameState> Immediately => RunAtTheStartOfEachInvadorPhase(this);

	bool IRunBeforeInvaderPhase.RemoveAfterRun => false;

	Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState )
		=> Cmd.ForEachSpirit(
			Cmd.DestroyPresence()
				.OnlyExecuteIf( self => 3 <= self.InPlay.Count || self.InPlay.Any( c => 4 <= c.Cost ) )
		).ActAsync( gameState );

	[ModuleInitializer]
	internal static void RegisterSerialization() {
		BlightCardRegistry.Register( nameof( PowerCorrodesTheSpirit ), ( json, ctx ) => new PowerCorrodesTheSpirit() );
		// Registers `this` as the IRunBeforeInvaderPhase entry - resolve to the live GameState.BlightCard,
		// not a fresh instance. See docs/GameSerialization-Roadmap.md section 10.
		PreInvaderPhaseActionRegistry.Register( nameof( PowerCorrodesTheSpirit ), ( json, ctx ) => (IRunBeforeInvaderPhase)ctx.BlightCard );
	}

}