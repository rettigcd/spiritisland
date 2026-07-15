namespace SpiritIsland.Basegame;

public class DownwardSpiral : BlightCard, IRunBeforeInvaderPhase {

	public DownwardSpiral():base("Downward Spiral", "At the start of each Invader Phase each Spirit destorys 1 of their presence.",5 ) {}

	public override IActOn<GameState> Immediately => RunAtTheStartOfEachInvadorPhase(this);

	bool IRunBeforeInvaderPhase.RemoveAfterRun => false;

	Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState )
		=> Cmd.DestroyPresence().ForEachSpirit().ActAsync( gameState );

	

}