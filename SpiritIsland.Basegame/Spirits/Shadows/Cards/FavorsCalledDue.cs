namespace SpiritIsland.Basegame;

public class FavorsCalledDue {

	[SpiritCard("Favors Called Due",1,Element.Moon,Element.Air,Element.Animal),Slow,FromPresence(1)]
	[Instructions( "Gather up to 4 Dahan. If Invaders are present and Dahan now outnumber them, 3 Fear." ), Artist( Artists.NolanNasser )]
	static public async Task Act(TargetSpaceCtx ctx){

		// gather up to 4 dahan
		await ctx.GatherUpToNDahan( 4 );

		// if invaders are present and dahan now out numberthem, 3 fear
		var invaderCount = ctx.Space.InvaderTotal();
		if(0 < invaderCount && invaderCount < ctx.Dahan.CountAll)
			await ctx.AddFear( 3 );

	}

}