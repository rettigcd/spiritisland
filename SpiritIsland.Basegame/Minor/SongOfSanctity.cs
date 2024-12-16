namespace SpiritIsland.Basegame;

public class SongOfSanctity {

	public const string Name = "Song of Sanctity";

	[MinorCard(SongOfSanctity.Name, 1, Element.Sun,Element.Water,Element.Plant),Slow,FromPresence(1,[Filter.Jungle, Filter.Mountain] )]
	[Instructions( "If target land has 1 Explorer, Push all Explorer. Otherwise, remove 1 Blight." ), Artist( Artists.NolanNasser )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){
		int explorerCount = ctx.Space.Sum( Human.Explorer );
		if( 0 < explorerCount )
			await ctx.Push( explorerCount, Human.Explorer);
		else 
			await ctx.RemoveBlight();
	}

}