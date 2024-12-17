namespace SpiritIsland.Basegame;

[InnatePower( Name ),SlowButFastIf("4 air")]
[FromPresence(1)]
public class GatherTheWarriors {

	public const string Name = "Gather the Warriors";

	[DisplayOnly( "4 air", "This Power may be fast." )]
	static public Task MayBeFastAsync(TargetSpaceCtx _ ) { return Task.CompletedTask; }

	[InnateTier( "1 animal,1 air", "Gather up to 1 dahan per air you have.", 0 )]
	static public async Task OptionAsync(TargetSpaceCtx ctx ) {
		int gatherCount = ctx.Self.Elements.Elements[Element.Air];
		await ctx.GatherUpToNDahan( gatherCount );
	}

	[InnateTier( "1 animal,1 sun", "Push up to 1 dahan per sun you have.", 1 )]
	static public async Task AAOptionAsync( TargetSpaceCtx ctx ) {
		int pushCount = ctx.Self.Elements.Elements[Element.Sun];
		await ctx.PushUpToNDahan( pushCount );
	}


}