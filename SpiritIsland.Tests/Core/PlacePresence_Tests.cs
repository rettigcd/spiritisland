using Shouldly;
using SpiritIsland.SinglePlayer;
using System;
using System.Collections.Generic;
using Xunit;

namespace SpiritIsland.Tests.Core {

	public class PlacePresence_Tests : GrowthTests {

		class TestSpirit : Spirit {

			public override string SpecialRules => throw new NotImplementedException();

			public TestSpirit():base(
				new SpiritPresence(
					new PresenceTrack( Track.Energy0, Track.Energy0, Track.Energy0 ),
					new PresenceTrack( Track.Card1, Track.Card2, Track.Card3, Track.Card4, Track.Card5 )
				)
			) {
				growthOptionGroup = new GrowthOptionGroup( new GrowthOption( new PlacePresence( 1, Target.Any ) ) );
			}

			public override string Text => "Test Spirit";

			protected override void InitializeInternal( Board _, GameState _1 ){
				throw new NotImplementedException();
			}

		}

		public PlacePresence_Tests():base(new TestSpirit()){
			Given_HasPresence(board[1]);
		}


		[Fact]
		public void PullsFrom_EnergyTrack() {
			// Given: spirit has one place presence action
			_ = When_Growing(0);

			User.PlacesEnergyPresence( "A1;A2" );

			spirit.Presence.Energy.RevealedCount.ShouldBe(2);
		}

		[Fact]
		public void PullsFrom_CardTrack(){
			_ = When_Growing( 0 );
			User.PlacesCardPlayPresence( "A1;A2" );
			spirit.Presence.CardPlays.RevealedCount.ShouldBe(2);
		}

	}
}
