namespace SpiritIsland.Basegame;

public class SteamVents {


	[MinorCard("Steam Vents", 1, "fire,air,water,earth"),Fast,FromPresence(0)]
	[Instructions( "Destroy 1 Explorer. -If you have- 3 Earth: You may instead destroy 1 Town." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		await ctx.SelectActionOption(
			new SpaceCmd(
				"Destroy 1 explorer", 
				ctx => ctx.Invaders.DestroyNOfClass( 1, Human.Explorer ) 
			),
			new SpaceCmd(
				"Destroy 1 town", 
				ctx => ctx.Invaders.DestroyNOfClass( 1, Human.Town )
			).OnlyExecuteIf( ctx.Tokens.Has(Human.Town) && await ctx.YouHave("3 earth") )
		);
	}

}