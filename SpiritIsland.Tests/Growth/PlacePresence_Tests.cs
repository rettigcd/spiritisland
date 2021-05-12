using System;
using System.Collections.Generic;
using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class PlacePresence_Tests : GrowthTests {

		class TestSpirit : Spirit {
			public override GrowthOption[] GetGrowthOptions( GameState gameState ) {
				return new GrowthOption[]{
					new GrowthOption(this.actions.ToArray())
				};
			}
			public List<GrowthAction> actions = new List<GrowthAction>();
		}

		public PlacePresence_Tests():base(new TestSpirit()){
			Given_HasPresence(board[1]);
		}


		[Fact]
		public void PullsFrom_EnergyTrack() {
			// Given: spirit has one place presence action
			Given_SpiritGrowsByPlacingPresence();
			When_Growing( 0, Resolve_PlacePresence( "A1" ) );
			Assert.Equal( 2, spirit.RevealedEnergySpaces );
		}

		[Fact]
		public void PullsFrom_CardTrack(){
			Given_SpiritGrowsByPlacingPresence();
			When_Growing( 0, Resolve_PlacePresence( "A1", Track.Card ) );
			Assert.Equal(2,spirit.RevealedCardSpaces);
		}

		[Fact]
		public void PullsFrom_EachTrack(){
			Given_SpiritGrowsByPlacingPresence(2);
			When_Growing( 0, Resolve_PlacePresence( "A1A1", Track.Energy, Track.Card ) );
			Assert.Equal(2,spirit.RevealedCardSpaces);
			Assert.Equal(2,spirit.RevealedEnergySpaces);
		}

		void Given_SpiritGrowsByPlacingPresence(int count=1) {
			bool spaceHasPresence(Space s)=>spirit.Presence.Contains(s);
			var criteria = new RangeCriteria( 1,spaceHasPresence );
			var criteriaList = new List<RangeCriteria>();
			while(count-->0)
				criteriaList.Add(criteria);
			(spirit as TestSpirit).actions.Add( new PlacePresence( spirit, criteriaList.ToArray() ) );
		}

	}
}
