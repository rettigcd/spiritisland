using System;
using System.Collections.Generic;
using SpiritIsland.Core;
using Xunit;

namespace SpiritIsland.Tests.Core {

	public class PlacePresence_Tests : GrowthTests {

		class TestSpirit : Spirit {

			public TestSpirit():base(
				new Track[]{ Track.Energy0, Track.Energy0, Track.Energy0 },
				new Track[] { Track.Card1, Track.Card2, Track.Card3, Track.Card4, Track.Card5 }
			) {

			}

			public override string Text => "Test Spirit";

			public override GrowthOption[] GetGrowthOptions() {
				return new GrowthOption[]{
					new GrowthOption(this.actions.ToArray())
				};
			}

			public override void Initialize( Board _, GameState _1 ){
				throw new NotImplementedException();
			}

			public List<GrowthActionFactory> actions = new();
		}

		public PlacePresence_Tests():base(new TestSpirit()){
			Given_HasPresence(board[1]);
		}


		[Fact]
		public void PullsFrom_EnergyTrack() {
			// Given: spirit has one place presence action
			Given_SpiritGrowsByPlacingPresence();
			When_Growing(0);
			Resolve_PlacePresence( "A1" );
			Assert.Equal( 2, spirit.RevealedEnergySpaces );
		}

		[Fact]
		public void PullsFrom_CardTrack(){
			Given_SpiritGrowsByPlacingPresence();
			When_Growing( 0 );
			Resolve_PlacePresence( "A1", spirit.NextCardPresence );
			Assert.Equal(2,spirit.RevealedCardSpaces);
		}

		void Given_SpiritGrowsByPlacingPresence(int count=1) {
			bool spaceHasPresence(Space s,GameState _)=>spirit.Presence.Contains(s);
			var testSpirit = spirit as TestSpirit;
			while(count-->0)
				testSpirit.actions.Add( new PlacePresence( 1, spaceHasPresence, "presence") );
		}

	}
}
