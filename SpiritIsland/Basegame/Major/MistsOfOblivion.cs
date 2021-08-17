using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class MistsOfOblivion {

		[MajorCard( "Mists of Oblivion", 4, Speed.Slow, Element.Moon, Element.Air, Element.Water )]
		[FromPresence(3)]
		static public async Task ActAsync( ActionEngine engine, Space target ) {
			var grp = engine.InvadersOn( target );
			var (self, _) = engine;

			var startingTownsAndCities = grp.TownsAndCitiesCount;
			// 1 damage to each invader
			await grp.ApplyDamageToEach(1,grp.InvaderTypesPresent_Generic.ToArray());

			// if you have 2 moon 3 air 2 water, 3 damage
			if(self.Elements.Contains("2 moon,3 air,2 water"))
				await grp.ApplySmartDamageToGroup(3);

			// 1 fear per town/city this power destroys (to a max of 4)
			int destroyedTownsAndCities = startingTownsAndCities - grp.TownsAndCitiesCount;
			engine.GameState.AddFear( destroyedTownsAndCities );
		}

	}
}
