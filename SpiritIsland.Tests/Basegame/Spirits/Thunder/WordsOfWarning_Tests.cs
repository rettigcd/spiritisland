using SpiritIsland.Basegame;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.Thunder {

	public class WordsOfWarning_Tests : ThunderCards  {

		[Fact]
		public async Task DeadDahanDoDamage() {

			gs.Phase = Phase.Fast;

			// Disable destroying presence
			gs.DetermineAddBlightEffect = (gs,space) => new AddBlightEffect { Cascade=false,DestroyPresence=false };

			// Given: 2 dahan on a2
			gs.DahanOn( a[2] ).Init(2);
			// and: dahan on a4 so it doesn't auto-select the only target available
			gs.DahanOn( a[4] ).Init(1);

			//  and: 4 explorers + 1 city
			var counts = gs.Tokens[ a[2] ];
			counts.Adjust( Invader.Explorer.Default, 4 );
			counts.Adjust( Invader.City.Default, 1 );
			// and activate card
			When_ActivateCard( WordsOfWarning.Name );
			
			User.TargetsLand(WordsOfWarning.Name,"(A2),A4");

			// When: ravaging on A2
			await gs.InvaderEngine.TestRavage(new InvaderCard(a[2]));

			// Then: 1 explorer left
			// Words of Warning defend 3 cancelling out City attack leaving only 4 damage from explorers
			// 2 Dahan attack simultaneously doing 4 points of damage, killing City and 1 explorer leaving 3 explorers
			gs.Assert_Invaders(a[2], "3E@1" );
		}

	}

}
