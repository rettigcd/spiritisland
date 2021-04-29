
namespace SpiritIsland {
	public class GainEnergy : GrowthAction {
		readonly int amount;
		public GainEnergy(Spirit spirit, int amount):base(spirit){
			this.amount = amount; 
		}

		public override void Apply() {
			spirit.Energy += amount;
		}
	}
}
