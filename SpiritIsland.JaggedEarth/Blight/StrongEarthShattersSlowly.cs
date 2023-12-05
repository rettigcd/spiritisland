namespace SpiritIsland.JaggedEarth;

public class StrongEarthShattersSlowly : StillHealthyBlightCard {

	const string NAME = "Strong Earth Shatters Slowly";
	const string DESCRIPTION = "Immediately, Each player adds 1 blight (from this card) to a land adjacent to blight.";

	public StrongEarthShattersSlowly():base(NAME, DESCRIPTION, 2) {}

	public override BaseCmd<GameState> Immediately 
		=> Cmd.AddBlightedIslandBlight
			.To().SpiritPickedLand().Which( Is.AdjacentToBlight )
			.ForEachSpirit();

}