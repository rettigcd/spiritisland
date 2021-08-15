using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class MistsOfOblivion {


		[MajorCard( "Mists of Oblivion", 4, Speed.Slow, Element.Moon, Element.Air, Element.Water )]
		[FromPresence(3)]
		static public Task ActAsync( ActionEngine engine, Space target ) {
			var (self, gs) = engine;
			var grp = gs.InvadersOn( target );
			var startingTownsAndCities = grp.TypeCount(Invader.Town,Invader.City);
			// 1 damage to each invader
			grp.ApplyDamageToEach(1,grp.HealthyInvaderTypesPresent.ToArray());

			// if you have 2 moon 3 air 2 water, 3 damage
			var el = self.Elements;
			if(2<=el[Element.Moon] && 3<=el[Element.Air] && 2<=el[Element.Water])
				grp.SmartDamageToGroup(3);

			// 1 fear per town/city this power destroys (to a max of 4)
			int destroyedTownsAndCities = startingTownsAndCities - grp.TypeCount(Invader.Town,Invader.City);
			engine.GameState.AddFear( destroyedTownsAndCities );

			return Task.CompletedTask;
		}

	}
}
