namespace SpiritIsland.Basegame;

public class MemoryFadesToDust : BlightCard, IRunBeforeInvaderPhase {

	public MemoryFadesToDust() : base( "Memory Fades to Dust", "At the start of each Invader Phase each Spirit Forgets a Power or destroys 1 of their presence.", 4 ) {}

	public override IActOn<GameState> Immediately => RunAtTheStartOfEachInvadorPhase(this);

	bool IRunBeforeInvaderPhase.RemoveAfterRun => false;

	Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState )
		=> Cmd.Pick1(
			Cmd.DestroyPresence(),
			Cmd.ForgetPowerCard
		)
		.ForEachSpirit()
		.ActAsync( gameState );

	[ModuleInitializer]
	internal static void RegisterSerialization() {
		BlightCardRegistry.Register( nameof( MemoryFadesToDust ), ( json, ctx ) => new MemoryFadesToDust() );
		// Registers `this` as the IRunBeforeInvaderPhase entry - resolve to the live GameState.BlightCard,
		// not a fresh instance. See docs/GameSerialization-Roadmap.md section 10.
		PreInvaderPhaseActionRegistry.Register( nameof( MemoryFadesToDust ), ( json, ctx ) => (IRunBeforeInvaderPhase)ctx.BlightCard );
	}

}