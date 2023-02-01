namespace SpiritIsland.Basegame;

public class DrawOfTheFruitfulEarth {

	[SpiritCard("Draw of the Fruitful Earth",1,Element.Earth,Element.Plant,Element.Animal)]
	[Slow]
	[FromPresence(1)]
	static public async Task Act(TargetSpaceCtx ctx){
		await ctx.GatherUpTo( 2, Human.Explorer );
		await ctx.GatherUpToNDahan( 2 );
	}

}