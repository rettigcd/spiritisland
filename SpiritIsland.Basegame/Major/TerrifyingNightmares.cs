namespace SpiritIsland.Basegame;

public class TerrifyingNightmares {

	[MajorCard("Terrifying Nightmares",4,Element.Moon,Element.Air),Fast,FromPresence(2)]
	[Instructions( "2 Fear. Push up to 4 Explorer / Town. -If you have- 4 Moon: +4 Fear." ), Artist( Artists.LoicBelliau )]
	static public async Task Act(TargetSpaceCtx ctx){

		// 2 fear
		await ctx.AddFear(2);

		// push up to 4 explorers or towns
		await ctx.PushUpTo(4, Human.Explorer_Town);

		// if you have 4 moon, +4 fear
		if( await ctx.YouHave("4 moon") )
			await ctx.AddFear(4);

	}

}