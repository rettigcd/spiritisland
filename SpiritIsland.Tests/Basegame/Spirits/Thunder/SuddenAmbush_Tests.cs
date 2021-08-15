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
			suddenAmbush.Cost.ShouldBe( 1 );
		}

		[Fact]
		public void NoDahanToGather() {
			When_ActivateCard( SuddenAmbush.Name );
			Step( "Select target.", "A1,A2,A4,A5,A6", a[2], true );
		}

		[Fact]
		public void Gather1_Kill1() {
			// Given: dahan on a3
			eng.GameState.AdjustDahan(a[3]);
			//  and: 2 explorers on a2
			eng.GameState.Adjust(a[2],Invader.Explorer,2);

			When_ActivateCard( SuddenAmbush.Name );
			Step( "Select target.", "A1,A2,A4,A5,A6", a[2], false );
			Step( "Gather dahan 1 of 1 from:", "A3,Done", a[3], true);

			// Then: 1 explorer left
			eng.GameState.InvadersOn( a[2] ).ToString().ShouldBe("1E@1");
		}

		[Fact]
		public void Gather1_Kills3() {
			// Given: 1 dahan on a2 & 2 dahan on a1
			eng.GameState.AdjustDahan( a[2] );
			eng.GameState.AdjustDahan( a[1], 2 );
			//  and: 5 explorers on a1
			eng.GameState.Adjust( a[1], Invader.Explorer, 5 );

			When_ActivateCard( SuddenAmbush.Name );
			Step( "Select target.", "A1,A2,A4,A5,A6", a[1], false );
			Step( "Gather dahan 1 of 1 from:", "A2,Done", a[2], true );

			// Then: 5-2-1 = 2 explorers left
			eng.GameState.InvadersOn( a[1] ).ToString().ShouldBe( "2E@1" );
		}

		[Fact]
		public void DoesntKillTown() {
			// Given: 1 dahan on a2 & 2 dahan on a1
			eng.GameState.AdjustDahan(a[2]);
			eng.GameState.AdjustDahan(a[1], 2);
			//  and: 1 town on a1
			eng.GameState.Adjust(a[1], Invader.Town, 1);

			When_ActivateCard(SuddenAmbush.Name);
			Step("Select target.", "A1,A2,A4,A5,A6", a[1], false);
			Step("Gather dahan 1 of 1 from:", "A2,Done", a[2], true);

			// Then: 5-2-1 = 2 explorers left
			eng.GameState.InvadersOn(a[1]).ToString().ShouldBe("1T@2");
		}

	}
}
