namespace SpiritIsland.Basegame;

public class Drought {

	[MinorCard( "Drought", 1, Element.Sun, Element.Fire, Element.Earth ),Slow,FromPresence(1)]
	[Instructions( "Destroy 3 Town. 1 Damage to each Town / City. Add 1 Blight. -If you have- 3 Sun: Destroy 1 City." ), Artist( Artists.NolanNasser )]
	static public async Task Act( TargetSpaceCtx ctx ) {

		// Destroy 3 towns.
		await ctx.Invaders.DestroyNOfClass( 3, Human.Town );

		// 1 damage to each town/city
		await ctx.DamageEachInvader( 1, Human.Town_City );

		// add 1 blight
		await ctx.AddBlight(1);

		// if you have 3 sun, destroy 1 city
		if( await ctx.YouHave("3 sun") )
			await ctx.Invaders.DestroyNOfClass( 1, Human.City );
	}

}
