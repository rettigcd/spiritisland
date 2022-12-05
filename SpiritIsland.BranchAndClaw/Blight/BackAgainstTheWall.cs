namespace SpiritIsland.BranchAndClaw;

public class BackAgainstTheWall : BlightCardBase {

	public BackAgainstTheWall():base("Back Against the Wall", 2 ) { }

	public override DecisionOption<GameCtx> Immediately => Cmd.EachSpirit( BoostEnergyAndCardPlayEachSpiritPhase );

	static SelfAction BoostEnergyAndCardPlayEachSpiritPhase => new SelfAction(
		"Each spirit phase, Gain +1 energy and +1 card play", 
		ctx => ctx.Self.EnergyCollected.ForGame.Add( BoostEnergyAndCardPlay )
	);

	static void BoostEnergyAndCardPlay( Spirit spirit ) {
		spirit.Energy++;
		spirit.tempCardPlayBoost++;
	}

}