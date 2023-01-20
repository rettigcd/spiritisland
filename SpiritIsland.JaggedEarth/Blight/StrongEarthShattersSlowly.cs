namespace SpiritIsland.JaggedEarth;

public class StrongEarthShattersSlowly : StillHealthyBlightCard {

	public StrongEarthShattersSlowly():base("Strong Earth Shatters Slowly","Immediately, Each player adds 1 blight (from this card) to a land adjacent to blight.", 2) {}

	public override DecisionOption<GameCtx> Immediately 
		// Each player
		=> Cmd.EachSpirit(
			// adds 1 blight (from this card)
			Cmd.AddBlightedIslandBlight
				// to a land adjacent to blight.
				.To( spaceCtx => spaceCtx.AdjacentCtxs.Any( adjCtx=>adjCtx.Tokens.Blight.Any ), "land adjacent to blight" )
		);

}