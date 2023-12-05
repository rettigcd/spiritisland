namespace SpiritIsland.BranchAndClaw;

public class BackAgainstTheWall : BlightCard {

	public BackAgainstTheWall():base("Back Against the Wall", "Every Spirit Phase each Spirit gains +1 Energy and +1 Card Play.", 2 ) { }

	public override BaseCmd<GameState> Immediately => Cmd.ForEachSpirit( BoostEnergyAndCardPlayEachSpiritPhase );

	static SpiritAction BoostEnergyAndCardPlayEachSpiritPhase => new SpiritAction(
		"Each spirit phase, Gain +1 energy and +1 card play", 
		self => self.EnergyCollected.Add( BoostEnergyAndCardPlay )
	);

	static void BoostEnergyAndCardPlay( Spirit spirit ) {
		spirit.Energy++;
		spirit.tempCardPlayBoost++;
	}

}