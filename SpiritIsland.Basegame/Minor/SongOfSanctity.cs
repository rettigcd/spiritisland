namespace SpiritIsland.Basegame;

public class SongOfSanctity {

	public const string Name = "Song of Sanctity";

	[MinorCard(SongOfSanctity.Name, 1, Element.Sun,Element.Water,Element.Plant)]
	[Slow]
	[FromPresence(1,Target.Jungle, Target.Mountain )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){
		int explorerCount = ctx.Tokens.Sum( Human.Explorer );
		if( 0 < explorerCount )
			await ctx.Push( explorerCount, Human.Explorer);
		else 
			await ctx.RemoveBlight();
	}

}