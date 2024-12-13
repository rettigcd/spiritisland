namespace SpiritIsland.Basegame;

public class VisionsOfFieryDoom {

	[MinorCard("Visions of Fiery Doom",1, Element.Moon,Element.Fire),Fast,FromPresence(0)]
	[Instructions( "1 Fear. Push 1 Explorer / Town. -If you have- 2 Fire: +1 Fear." ), Artist( Artists.LucasDurham )]
	static public async Task Act(TargetSpaceCtx ctx){
		// 1 fear
		await ctx.AddFear( 1 );

		// Push 1 explorer/town
		await ctx.Push( 1, Human.Explorer_Town );

		if(await ctx.YouHave("2 fire"))
			await ctx.AddFear( 1 );
	}

}