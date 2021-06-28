
namespace SpiritIsland.Core {

	public class GainEnergy : GrowthActionFactory {

		readonly int amount;
		public GainEnergy(int amount){
			this.amount = amount; 
		}

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new ResolvedAction( ()=>spirit.Energy += amount );
		}

	}

}
