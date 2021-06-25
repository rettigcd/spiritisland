namespace SpiritIsland {
	public class ReclaimAll : GrowthAction {

		public override void Apply(){
			spirit.Hand.AddRange( spirit.DiscardPile );
			spirit.DiscardPile.Clear();
		}

		public override bool IsResolved => true;

		public override IOption[] Options => throw new System.NotImplementedException();
	}


}
