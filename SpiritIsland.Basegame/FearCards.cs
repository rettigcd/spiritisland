namespace SpiritIsland.Basegame;

public class FearCards {

	public static IFearCard[] GetFearCards() {
		return new IFearCard[] {
			new AvoidTheDahan(),
			new BeliefTakesRoot(),
			new DahanEnheartened(),
			new DahanOnTheirGuard(),
			new DahanRaid(),
			new EmigrationAccelerates(),
			new FearOfTheUnseen(),
			new Isolation(),
			new OverseasTradeSeemsSafer(),
			new Retreat(),
			new Scapegoats(),
			new SeekSafety(),
			new TallTalesOfSavagery(),
			new TradeSuffers(),
			new WaryOfTheInterior()
		};
	}

}