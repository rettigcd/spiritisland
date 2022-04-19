using Shouldly;
using SpiritIsland.Basegame;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.Thunder {

	public class SuddenAmbush_Tests : ThunderCards {

		public SuddenAmbush_Tests() {
			gs.Phase = Phase.Fast;
		}

		[Fact]
		public void IsFastAndCost1() {
			var suddenAmbush = PowerCard.For<SuddenAmbush>();
			suddenAmbush.DisplaySpeed.ShouldBe(Phase.Fast);
			suddenAmbush.Cost.ShouldBe( 2 );
		}

		[Fact]
		public void NoDahanToGather() {
			When_ActivateCard( SuddenAmbush.Name );
			User.TargetsLand( SuddenAmbush.Name, "A1,(A2),A4,A5,A6");
		}

		[Fact]
		public void Gather1_Kill1() {
			// Given: dahan on a3
			gs.DahanOn(a[3]).Init(1);
			//  and: 2 explorers on a2
			gs.Tokens[a[2]].AdjustDefault(Invader.Explorer,2);

			When_ActivateCard( SuddenAmbush.Name );
			User.TargetsLand(SuddenAmbush.Name, "A1,(A2),A4,A5,A6");
			User.GathersOptionalToken("D@2 on A3");

			// Then: 1 explorer left
			gs.Assert_Invaders( a[2], "1E@1" );
		}

		[Fact]
		public void Gather1_Kills3() {
			// Given: 1 dahan on a2 & 2 dahan on a1
			gs.DahanOn( a[2] ).Init(1);
			gs.DahanOn( a[1] ).Init(2);
			//  and: 5 explorers on a1
			gs.Tokens[a[1]].AdjustDefault( Invader.Explorer, 5 );

			When_ActivateCard( SuddenAmbush.Name );

			User.TargetsLand(SuddenAmbush.Name,"(A1),A2,A4,A5,A6");
			User.GathersOptionalToken("D@2 on A2");

			// Then: 5-2-1 = 2 explorers left
			gs.Assert_Invaders( a[1], "2E@1" );
		}

		[Trait("Feature","Gather")]
		[Fact]
		public void DoesntKillTown() {
			// Given: 1 dahan on a2 & 2 dahan on a1
			gs.DahanOn(a[2]).Init(1);
			gs.DahanOn(a[1]).Init(2);
			//  and: 1 town on a1
			gs.Tokens[a[1]].AdjustDefault(Invader.Town, 1);

			When_ActivateCard(SuddenAmbush.Name);

			User.TargetsLand(SuddenAmbush.Name,"(A1),A2,A4,A5,A6");
			User.GathersOptionalToken("D@2 on A2");

			// Then: 5-2-1 = 2 explorers left
			gs.Assert_Invaders(a[1], "1T@2" );
		}

	}
}
