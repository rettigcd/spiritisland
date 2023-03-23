namespace SpiritIsland.JaggedEarth;
public class SetThemOnAnEverTwistingTrail{ 

	[MinorCard("Set Them on an Ever-Twisting Trail",1,Element.Air,Element.Plant,Element.Animal),Fast,FromSacredSite(1)]
	[Instructions( "Gather or Push 1 Explorer. Isolate target land." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// Gather or Push 1 Explorer.
		await ctx.SelectActionOption( Cmd.GatherUpToNExplorers(1), Cmd.PushUpToNExplorers(1) );

		// Isolate target land.
		ctx.Isolate();
	}

}
