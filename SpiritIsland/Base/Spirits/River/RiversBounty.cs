using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class RiversBounty {

		public const string Name = "River's Bounty";
		[SpiritCard(RiversBounty.Name, 0, Speed.Slow,Element.Sun,Element.Water,Element.Animal)]
		static public async Task ActionAsync(ActionEngine eng) {
			var (self, gameState) = eng;

			bool HasCloseDahan( Space space ) => space.SpacesWithin( 1 ).Any( gameState.HasDahan );
			var target = await eng.Api.TargetSpace_Presence(0,HasCloseDahan );

			// Gather up to 2 Dahan
			await eng.GatherUpToNDahan( target, 2 );

			// If there are now at least 2 dahan, then add 1 dahan and gain 1 energy
			if(gameState.GetDahanOnSpace( target ) >= 2) {
				gameState.AddDahan( target, 1 );
				++self.Energy;
			}
		}

	}

}



