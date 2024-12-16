namespace SpiritIsland.Basegame;

public class DrawOfTheFruitfulEarth {

	[SpiritCard("Draw of the Fruitful Earth",1,Element.Earth,Element.Plant,Element.Animal),Slow,FromPresence(1)]
	[Instructions( "Gather up to 2 Explorer. Gather up to 2 Dahan." ), Artist( Artists.SydniKruger )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		await ctx.GatherUpTo( 2, Human.Explorer );
		await ctx.GatherUpToNDahan( 2 );
	}

}