namespace SpiritIsland.JaggedEarth;

public class StrongEarthShattersSlowly : StillHealthyBlightCard {

	public StrongEarthShattersSlowly():base("Strong Earth Shatters Slowly",2) {}

	public override ActionOption<GameState> Immediately 
		// Each player adds 1 blight (from this card) to a land adjacent to blight.
		=> Cmd.EachSpirit( Cmd.AddBlightedIslandBlight.To( spaceCtx => spaceCtx.AdjacentCtxs.Any( adjCtx=>adjCtx.Tokens.Blight.Any ), "land adjacent to blight" )
		);

}