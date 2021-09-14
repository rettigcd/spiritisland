namespace SpiritIsland.Basegame {

	public class FearCards {

		public static IFearOptions[] GetFearCards() {
			return new IFearOptions[] {
				new AvoidTheDahan(),
				new BeliefTakesRoot(),
				new DahanEnheartened(),
				new DahanOnTheirGuard(),
				new DahanRaid(),
				new EmigrationAccelerates(),
				new FearOfTheUnseen(),
				new Isolation(),
				new OverseasTradeSeemSafer(),
				new Retreat(),
				new Scapegoats(),
				new SeekSafety(),
				new TallTalesOfSavagery(),
				new TradeSuffers(),
				new WaryOfTheInterior()
			};
		}

	}

}

