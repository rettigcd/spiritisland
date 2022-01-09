namespace SpiritIsland.BranchAndClaw {

	public class BackAgainstTheWall : BlightCardBase {

		public BackAgainstTheWall():base("Back Against the Wall", 2 ) { }

		public override ActionOption<GameState> Immediately => Cmd.EachSpirit(Cause.Blight, BoostEnergyAndCardPlayEachSpiritPhase );

		static SelfAction BoostEnergyAndCardPlayEachSpiritPhase => new SelfAction(
			"Each spirit phase, Gain +1 energy and +1 card play", 
			ctx => ctx.Self.EnergyCollected += BoostEnergyAndCardPlay
		);

		static void BoostEnergyAndCardPlay( Spirit spirit ) {
			spirit.Energy++;
			spirit.tempCardPlayBoost++;
		}
	}

}
