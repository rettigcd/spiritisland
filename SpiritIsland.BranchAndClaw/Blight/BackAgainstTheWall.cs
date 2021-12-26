using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class BackAgainstTheWall : BlightCardBase {

		public BackAgainstTheWall():base("Back Against the Wall", 2 ) { }

		// Every spirit phase each spirit gains +1 energy and +1 card play

		// 2 blight per player
		protected override Task BlightAction( GameState gs ) {
			return GameCmd.EachSpirit(Cause.Blight, BoostEnergyAndCardPlayEachSpiritPhase ).Execute( gs );
		}

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
