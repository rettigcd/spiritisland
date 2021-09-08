using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class MistsOfOblivion {

		[MajorCard( "Mists of Oblivion", 4, Speed.Slow, Element.Moon, Element.Air, Element.Water )]
		[FromPresence(3)]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			var grp = ctx.PowerInvaders;

			int startingTownsAndCities = grp.Counts.TownsAndCitiesCount();
			// 1 damage to each invader
			await grp.ApplyDamageToAllTokensOfType(1,grp.Counts.Invaders());

			// if you have 2 moon 3 air 2 water
			if(ctx.Self.Elements.Contains("2 moon,3 air,2 water"))
				// 3 damage
				await grp.ApplySmartDamageToGroup(3);

			// 1 fear per town/city this power destroys (to a max of 4)
			int destroyedTownsAndCities = startingTownsAndCities - grp.Counts.TownsAndCitiesCount();
			ctx.AddFear( destroyedTownsAndCities );
		}

	}
}
