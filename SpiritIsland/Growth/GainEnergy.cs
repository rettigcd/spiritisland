
namespace SpiritIsland {
	public class GainEnergy : GrowthAction {
		readonly int amount;

		public GainEnergy(int amount){ this.amount = amount; }

		public override void Apply( PlayerState ps) {
			ps.Energy += amount;
		}
	}
}
