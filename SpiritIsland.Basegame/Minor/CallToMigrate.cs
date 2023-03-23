namespace SpiritIsland.Basegame;

public class CallToMigrate {

	public const string Name = "Call to Migrate";

	[MinorCard(Name,1,Element.Fire,Element.Air,Element.Animal),Slow,FromPresence(1)]
	[Instructions( "Gather up to 3 Dahan. Push up to 3 Dahan." ), Artist( Artists.GrahamStermberg )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		await ctx.GatherUpToNDahan(3);
		await ctx.PushUpToNDahan(3);
	}

}