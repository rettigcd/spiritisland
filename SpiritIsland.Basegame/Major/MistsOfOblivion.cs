using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class MistsOfOblivion {

		[MajorCard( "Mists of Oblivion", 4, Speed.Slow, Element.Moon, Element.Air, Element.Water )]
		[FromPresence(3)]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			int startingTownsAndCities = ctx.Tokens.TownsAndCitiesCount();

			// 1 damage to each invader
			await ctx.Invaders.ApplyDamageToAllTokensOfType(1,ctx.Tokens.Invaders());

			// if you have 2 moon 3 air 2 water
			if(ctx.YouHave("2 moon,3 air,2 water"))
				// 3 damage
				await ctx.Invaders.ApplySmartDamageToGroup(3);

			// 1 fear per town/city this power destroys (to a max of 4)
			int destroyedTownsAndCities = startingTownsAndCities - ctx.Tokens.TownsAndCitiesCount();
			ctx.AddFear( destroyedTownsAndCities );
		}

	}
}
