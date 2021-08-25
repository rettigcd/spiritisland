using Shouldly;
using SpiritIsland.SinglePlayer;
using System;
using System.Collections.Generic;
using Xunit;

namespace SpiritIsland.Tests.Core {

	public class PlacePresence_Tests : GrowthTests {

		class TestSpirit : Spirit {

			public TestSpirit():base(
				new MyPresence(
					new PresenceTrack( Track.Energy0, Track.Energy0, Track.Energy0 ),
					new PresenceTrack( Track.Card1, Track.Card2, Track.Card3, Track.Card4, Track.Card5 )
				)
			) {

			}

			public override string Text => "Test Spirit";

			public override (GrowthOption[],int) GetGrowthOptions() {
				var x = new GrowthOption[]{
					new GrowthOption( this.actions.ToArray())
				};
				return (x,1);
			}

			protected override void InitializeInternal( Board _, GameState _1 ){
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
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			Resolve_PlacePresence( "A1;A2", spirit.Presence.Energy.Next );

			spirit.Presence.Energy.RevealedCount.ShouldBe(2);
		}

		[Fact]
		public void PullsFrom_CardTrack(){
			Given_SpiritGrowsByPlacingPresence();
			When_Growing( 0 );
			_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			Resolve_PlacePresence( "A1;A2", spirit.Presence.CardPlays.Next );
			spirit.Presence.CardPlays.RevealedCount.ShouldBe(2);
		}

		void Given_SpiritGrowsByPlacingPresence(int count=1) {
			// bool spaceHasPresence(Space s,GameState _)=>spirit.Presence.Contains(s);
			var testSpirit = spirit as TestSpirit;
			while(count-->0)
				testSpirit.actions.Add( new PlacePresence( 1, Target.Any, "presence") );
		}

	}
}
