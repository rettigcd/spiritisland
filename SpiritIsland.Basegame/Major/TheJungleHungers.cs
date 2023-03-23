namespace SpiritIsland.Basegame;

public class TheJungleHungers {

	[MajorCard("The Jungle Hungers",3,Element.Moon,Element.Plant),Slow,FromPresenceIn(1,Terrain.Jungle)]
	[Instructions( "Destroy all Explorer and all Town. Destroy all Dahan. -If you have- 2 Moon, 3 Plant: Destroy 1 City. Do not destroy any Dahan." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// destroys all explorers and towns
		await ctx.Invaders.DestroyAll( Human.Explorer);
		await ctx.Invaders.DestroyAll( Human.Town );

		// Destroy all dahan
		bool destroyAllDahan = true;

		// if you have 2 moon, 3 plant
		if(await ctx.YouHave( "2 moon,3 plant" )) {
			// Destroy 1 city
			await ctx.Invaders.DestroyNOfClass( 1, Human.City );
			// do not destroy dahan
			destroyAllDahan = false;
		}

		if(destroyAllDahan)
			await ctx.Dahan.Destroy( ctx.Dahan.CountAll );

	}

}