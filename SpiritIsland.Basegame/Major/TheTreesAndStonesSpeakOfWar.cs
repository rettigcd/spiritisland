namespace SpiritIsland.Basegame;

public class TheTreesAndStonesSpeakOfWar {

	[MajorCard( "The Trees and Stones Speak of War", 2, Element.Sun, Element.Earth, Element.Plant )]
	[Fast]
	[FromPresence(1,Target.Dahan)]
	static public async Task ActionAsync( TargetSpaceCtx ctx ) {

		// for each dahan in target land, 1 damage and defend 2

		// -- damage --
		await ctx.DamageInvaders( ctx.Dahan.Count );

		// if you have 2 sun, 2 earth, 2 plant
		if( await ctx.YouHave("2 sun, 2 earth, 2 plant")) {
			// you may push up to 2 dahan
			Space[] dest = await ctx.PushUpToNDahan( 2 );
			// defend pushed
			foreach(var d in dest)
				ctx.Target(d).Defend( 2 );
		}

		// -- defend remaining --
		ctx.Defend( ctx.Dahan.Count * 2 );

	}

}