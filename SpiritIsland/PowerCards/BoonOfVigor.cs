namespace SpiritIsland.PowerCards {

	[PowerCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
	public class BoonOfVigor : IAction{

		public const string Name = "Boon of Vigor";

		public BoonOfVigor(Spirit self,GameState _){
			this.self = self;
		}

		public Spirit Target { get; set; }

		public bool IsResolved => Target != null;

		public void Apply() {
			this.Target.Energy += (Target==self) ? 1 : Target.ActiveCards.Count;
		}

		public IOption[] GetOptions() {
			throw new System.NotImplementedException();
		}

		public void Select(IOption option) {
			throw new System.NotImplementedException();
		}

		readonly Spirit self;

	}

}