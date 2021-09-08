using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.Thunder {

	public class SuddenAmbush_Tests : ThunderCards {

		[Fact]
		public void IsFastAndCost1() {
			var suddenAmbush = PowerCard.For<SuddenAmbush>();
			suddenAmbush.Speed.ShouldBe(Speed.Fast);
			suddenAmbush.Cost.ShouldBe( 2 );
		}

		[Fact]
		public void NoDahanToGather() {
			When_ActivateCard( SuddenAmbush.Name );
			Step( "Select space to target.", "A1,A2,A4,A5,A6", a[2], true );
		}

		[Fact]
		public void Gather1_Kill1() {
			// Given: dahan on a3
			gs.DahanAdjust(a[3]);
			//  and: 2 explorers on a2
			gs.Tokens[a[2]].Adjust(Invader.Explorer[1],2);

			When_ActivateCard( SuddenAmbush.Name );
			Step( "Select space to target.", "A1,A2,A4,A5,A6", a[2], false );
			Step( "Gather Dahan (1 remaining)", "A3,Done", a[3], true);

			// Then: 1 explorer left
			gs.Assert_Invaders( a[2], "1E@1" );
		}

		[Fact]
		public void Gather1_Kills3() {
			// Given: 1 dahan on a2 & 2 dahan on a1
			gs.DahanAdjust( a[2] );
			gs.DahanAdjust( a[1], 2 );
			//  and: 5 explorers on a1
			gs.Tokens[a[1]].Adjust( Invader.Explorer[1], 5 );

			When_ActivateCard( SuddenAmbush.Name );
			Step( "Select space to target.", "A1,A2,A4,A5,A6", a[1], false );
			Step( "Gather Dahan (1 remaining)", "A2,Done", a[2], true );

			// Then: 5-2-1 = 2 explorers left
			gs.Assert_Invaders( a[1], "2E@1" );
		}

		[Fact]
		public void DoesntKillTown() {
			// Given: 1 dahan on a2 & 2 dahan on a1
			gs.DahanAdjust(a[2]);
			gs.DahanAdjust(a[1], 2);
			//  and: 1 town on a1
			gs.Tokens[a[1]].Adjust(Invader.Town.Default, 1);

			When_ActivateCard(SuddenAmbush.Name);
			Step("Select space to target.", "A1,A2,A4,A5,A6", a[1], false);
			Step("Gather Dahan (1 remaining)", "A2,Done", a[2], true);

			// Then: 5-2-1 = 2 explorers left
			gs.Assert_Invaders(a[1], "1T@2" );
		}

	}
}
