namespace SpiritIsland.Basegame;

public class MistsOfOblivion {

	[MajorCard( "Mists of Oblivion", 4, Element.Moon, Element.Air, Element.Water )]
	[Slow]
	[FromPresence(3)]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		int startingTownsAndCities = ctx.Tokens.TownsAndCitiesCount();

		// 1 damage to each invader
		await ctx.DamageEachInvader(1);

		// if you have 2 moon 3 air 2 water
		if(await ctx .YouHave("2 moon,3 air,2 water"))
			// 3 damage
			await ctx.DamageInvaders(3);

		// 1 fear per town/city this power destroys (to a max of 4)
		int destroyedTownsAndCities = startingTownsAndCities - ctx.Tokens.TownsAndCitiesCount();
		ctx.AddFear( destroyedTownsAndCities );
	}

}