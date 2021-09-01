using Shouldly;
using SpiritIsland.Basegame;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.Thunder {

	public class WordsOfWarning_Tests : ThunderCards  {

		[Fact]
		public async Task DeadDahanDoDamage() {

			// Given: 2 dahan on a2
			gs.Dahan.Adjust( a[2], 2 );
			// and: dahan on a4 so it doesn't auto-select the only target available
			gs.Dahan.Adjust( a[4] );

			//  and: 4 explorers + 1 city
			var counts = gs.Invaders.Counts[ a[2] ];
			counts.Add( Invader.Explorer, 4 );
			counts.Add( Invader.City );
			// and activate card
			When_ActivateCard( WordsOfWarning.Name );
			Step( "Select space to target.", "A2,A4", a[2], true );

			// When: ravaging on A2
			await gs.Ravage(new InvaderCard(a[2].Terrain));

			// Then: 1 explorer left
			// Words of Warning defend 3 cancelling out City attack leaving only 4 damage from explorers
			// 2 Dahan attack simultaneously doing 4 points of damage, killing City and 1 explorer leaving 3 explorers
			gs.Assert_Invaders(a[2], "3E@1" );
		}

	}

}
