namespace SpiritIsland.JaggedEarth;

public class StrongEarthShattersSlowly : StillHealthyBlightCard {

	public StrongEarthShattersSlowly():base("Strong Earth Shatters Slowly",2) {}

	public override DecisionOption<GameState> Immediately 
		// Each player
		=> Cmd.EachSpirit(
			// adds 1 blight (from this card)
			Cmd.AddBlightedIslandBlight
				// to a land adjacent to blight.
				.To( spaceCtx => spaceCtx.AdjacentCtxs.Any( adjCtx=>adjCtx.Tokens.Blight.Any ), "land adjacent to blight" )
		);

}