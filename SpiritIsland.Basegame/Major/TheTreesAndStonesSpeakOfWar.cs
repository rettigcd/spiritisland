namespace SpiritIsland.Basegame;

public class TheTreesAndStonesSpeakOfWar {

	[MajorCard( "The Trees and Stones Speak of War", 2, Element.Sun, Element.Earth, Element.Plant ),Fast,FromPresence(1,Target.Dahan)]
	[Instructions( "For each Dahan in target land, 1 Damage and Defend 2. -If you have- 2 Sun, 2 Earth, 2 Plant: You may Push up to 2 Dahan, moving each's Defend with them." ), Artist( Artists.GrahamStermberg )]
	static public async Task ActionAsync( TargetSpaceCtx ctx ) {

		// for each dahan in target land, 1 damage and defend 2

		// -- damage --
		await ctx.DamageInvaders( ctx.Dahan.CountAll );

		// if you have 2 sun, 2 earth, 2 plant
		if( await ctx.YouHave("2 sun, 2 earth, 2 plant")) {
			// you may push up to 2 dahan
			await ctx.SourceSelector
				.AddGroup( 2, Human.Dahan )
				.ConfigDestination( Distribute.OnEachDestinationLand( to => to.Defend.Add(2) ) )
				.PushUpToN(ctx.Self );
		}

		// -- defend remaining --
		ctx.Defend( ctx.Dahan.CountAll * 2 );

	}

}