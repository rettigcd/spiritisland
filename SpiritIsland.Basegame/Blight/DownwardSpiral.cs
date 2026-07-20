namespace SpiritIsland.Basegame;

public class DownwardSpiral : BlightCard, IRunBeforeInvaderPhase {

	public DownwardSpiral():base("Downward Spiral", "At the start of each Invader Phase each Spirit destorys 1 of their presence.",5 ) {}

	public override IActOn<GameState> Immediately => RunAtTheStartOfEachInvadorPhase(this);

	bool IRunBeforeInvaderPhase.RemoveAfterRun => false;

	Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState )
		=> Cmd.DestroyPresence().ForEachSpirit().ActAsync( gameState );

	[ModuleInitializer]
	internal static void RegisterSerialization() {
		BlightCardRegistry.Register( nameof( DownwardSpiral ), ( json, ctx ) => new DownwardSpiral() );
		// Registers `this` as the IRunBeforeInvaderPhase entry (see Immediately above), so resolving it
		// from _preInvaderPhaseActions must return the live GameState.BlightCard, not a fresh instance
		// via BlightCardRegistry - see docs/GameSerialization-Roadmap.md section 10.
		PreInvaderPhaseActionRegistry.Register( nameof( DownwardSpiral ), ( json, ctx ) => (IRunBeforeInvaderPhase)ctx.BlightCard );
	}

}