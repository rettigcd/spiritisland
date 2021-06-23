
namespace SpiritIsland {

	public class GainEnergy : GrowthAction {

		readonly int amount;
		public GainEnergy(int amount){
			this.amount = amount; 
		}

		public override void Apply() {
			spirit.Energy += amount;
			spirit.MarkResolved( this );
		}

		public override bool IsResolved => true;

		public override IOption[] Options => throw new System.NotImplementedException();
	}
}
