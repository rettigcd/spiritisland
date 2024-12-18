namespace SpiritIsland.NatureIncarnate;

public class ShatteredFragmentsOfPower : BlightCard {

	public ShatteredFragmentsOfPower()
		:base("Shattered Fragments of Power", 
			"Immediately: Draw 1 Major Power Card per Spirit plus 2 more. Each Spirit Takes 1 and gains 2 Energy.", 
			2
		) 
	{}

	public override IActOn<GameState> Immediately => SpiritsGainMajorAndEnergy();

	static public IActOn<GameState> SpiritsGainMajorAndEnergy() => new BaseCmd<GameState>( 
		$"Draw 1 Major Power Card per Spirit plus 2 more. Each Spirit Takes 1 and gains 2 Energy.",
		async gs => {
			var majors = gs.MajorCards!.Flip(gs.Spirits.Length+2);
			foreach(var spirit in gs.Spirits) {
				// Major
				PowerCard card = await spirit.PickOutCard(majors);
				spirit.Hand.Add(card);
				majors.Remove(card);
				// Energy
				spirit.Energy += 2;
			}
			gs.MajorCards.Discard(majors);
		}
	);

}

