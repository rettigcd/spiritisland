namespace SpiritIsland.Basegame;

public class DownwardSpiral : BlightCard {

	public DownwardSpiral():base("Downward Spiral", "At the start of each Invader Phase each Spirit destorys 1 of their presence.",5 ) {}

	public override IActOn<GameState> Immediately 
		=> Cmd.DestroyPresence().ForEachSpirit().AtTheStartOfEachInvaderPhase();

}