namespace SpiritIsland.PowerCards {

	[PowerCard("Boon of Vigor", 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
	public class BoonOfVigor : IAction{

		public const string Name = "Boon of Vigor";

		readonly Spirit self;

		public BoonOfVigor(Spirit self,GameState _){
			this.self = self;
		}

		public Spirit Target { get; set; }

		public bool IsResolved => throw new System.NotImplementedException();

		public void Apply() {
			this.Target.Energy += (Target==self) ? 1 : Target.ActiveCards.Count;
		}
	}

}