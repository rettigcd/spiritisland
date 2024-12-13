namespace SpiritIsland.Basegame;

public class ShadowsOfTheBurningForest {

	[MinorCard("Shadows of the Burning Forest",0,Element.Moon,Element.Fire,Element.Plant),Slow,FromPresence(0,Filter.Invaders)]
	[Instructions( "2 Fear. If target land is Mountain / Jungle, Push 1 Explorer and 1 Town." ), Artist( Artists.NolanNasser )]
	static public async Task Act(TargetSpaceCtx ctx){

		// 2 fear
		await ctx.AddFear(2);

		// if target is M/J, Push 1 explorer and 1 town
		if(ctx.IsOneOf( Terrain.Mountain, Terrain.Jungle )) {
			await ctx.SourceSelector
				.AddGroup( 1, Human.Town )
				.AddGroup( 1, Human.Explorer )
				.PushN(ctx.Self);
		}

	}

}