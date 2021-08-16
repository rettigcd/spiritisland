using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Basegame;
using SpiritIsland;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits {

	public class Ocean_GrowthTests : GrowthTests {


		static Spirit GetOcean() {
			var ocean = new Ocean();
			ocean.CardDrawer = new IncrementCountCardDrawer();
			return ocean;
		}

		public Ocean_GrowthTests():base(GetOcean()){}

		[Theory]
		[InlineData("A0","","A0")]
		[InlineData("A0B0","","A0B0")]
		[InlineData("A0B0C0","","A0B0C0")]
		[InlineData("A1","A1>A0","A0")]
		[InlineData("A1B1","A1>A0,B1>B0","A0B0")]
		[InlineData("A1B1C1","A1>A0,B1>B0,C1>C0","A0B0C0")]
		[InlineData("A1A2","A1>A0","A0A2")]    // need to define which presence to move
		[InlineData("A1A2","A2>A0","A0A1")]    // need to define which presence to move
		[InlineData("A1A2B1C1C2","A2>A0,B1>B0,C1>C0","A0A1B0C0C2")]    // need to define which presence to move
		public void ReclaimGather_GatherParts(string starting, string select, string ending) {
			Given_IslandIsABC();
			Given_HasPresence( starting );

			When_Growing( 0 );

			// since options are move source, key on that
			var moveBySrc = select.Split(',')
				.Where(x=>!string.IsNullOrEmpty(x))
				.Select(s=>s.Split('>'))
				.ToDictionary(a=>a[0],a=>a[1]);

			var gather = spirit.GetUnresolvedActionFactories(Speed.Growth).OfType<GatherPresenceIntoOcean>().SingleOrDefault();

			if(gather != null){
				var engine = new ActionEngine( spirit, gameState );
				_ = gather.Activate( engine );
				while(!spirit.Action.IsResolved){
					var source = spirit.Action.Options.Single(x=>moveBySrc.ContainsKey(x.Text));
					spirit.Action.Select(source);
				}
			}

			// Then: nothing to gather
			Assert_BoardPresenceIs( ending );
		}


		[Theory]
		[InlineData("A1A2")]    // need to define which presence to move
		public void ReclaimGather_GatherParts_Unresolved(string starting){

			// Given: 3-board island
			gameState.Island = new Island(BoardA,BoardB,BoardC);

			Given_HasPresence( starting );

			// Changed implementation to not run unresolved things
			// !!! should assert that unresolved actions has 1 item in it.
			// Assert.Throws<InvalidOperationException>(()=>When_Growing(0));
		}

		[Fact]
		public void ReclaimGather_NonGatherParts(){
			// reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
			
			Given_HalfOfPowercardsPlayed();
			When_Growing(0);
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
			Assert_HasEnergy(2);
		}

		[Fact]
		public void TwoPresenceInOceans(){
			// +1 presence range any ocean, +1 presense in any ociean, +1 energy

			// Given: island has 2 boards, hence 2 oceans
			gameState.Island = new Island(BoardA,BoardB);

			When_Growing(1);
			Resolve_PlacePresenceInOcean( "A0;B0", spirit.Presence.Energy.Next);
			
			Assert_HasEnergy(1);
		}

		protected void Resolve_PlacePresenceInOcean( string placeOptions, Track source) {
			var ppFactory = spirit.GetUnresolvedActionFactories( Speed.Growth ).OfType<PlaceInOcean>()
				.First();
			var engine = new ActionEngine( spirit, gameState );
			ppFactory.Activate( engine );
			var ppAction = spirit.Action;

			// take from precense track
			ppAction.Select( source );

			// place on board - first option
			string[] options = placeOptions.Split( ';' );
			if(options.Length > 1) // not auto selecting
				ppAction.Select( ppAction.Options.Single( o => o.Text == options[0] ) );

			spirit.RemoveUnresolvedFactory( ppFactory );
		}


		[Theory]
		[InlineData("A0","A0>A2","A1;A2;A3","A1A2")]
		public void PowerPlaceAndPush( string starting, string pushStr, string placeOptions, string ending ){
			// gain power card
			// push 1 presense from each ocean
			// add presense on costal land range 1
			gameState.Island = new Island(BoardA,BoardB,BoardC);
			Given_HasPresence( starting );

			When_Growing(2);
			Resolve_PlacePresence( placeOptions);

			var targets = pushStr.Split(',')
				.Where(s=>!string.IsNullOrEmpty(s))
				.Select(s=>s.Split('>')[1])
				.ToArray();

			var push = spirit.GetUnresolvedActionFactories(Speed.Growth).OfType<PushPresenceFromOcean>().SingleOrDefault();

			if(push != null){
				var engine = new ActionEngine( spirit, gameState );
				_ = push.Activate( engine );
				var action = spirit.Action;
				while(!action.IsResolved){
					var options = action.Options;
					var target = options.Single(t=>targets.Contains(t.Text));
					action.Select(target);
				}
			}


			// !! fix
			//string[] p = push.Split( '>' );
			//var action = spirit.UnresolvedActionFactories.OfType<PushPresence>()
			//	.First(act=>act.From.Label==p[0]);
			//Assert.Equal(placeOptions,action.Options.Select(o=>o.Text).OrderBy(x=>x).Join(";"));
			//action.Select(action.From.SpacesWithin(1).Single(s=>s.Label==p[1]));
			//action.Apply();

			Assert_GainPowercard(1);
			Assert_BoardPresenceIs(ending);
		}

		[Theory]
		[InlineDataAttribute(1,0,"")]
		[InlineDataAttribute(2,0,"M")]
		[InlineDataAttribute(3,0,"MW")]
		[InlineDataAttribute(4,1,"MW")]
		[InlineDataAttribute(5,1,"MWE")]
		[InlineDataAttribute(6,1,"MWEW")]
		[InlineDataAttribute(7,2,"MWEW")]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			// energy: 0 moon water 1 earth water 2
			spirit.Presence.Energy.RevealedCount = revealedSpaces;
			Assert_EnergyTrackIs( expectedEnergyGrowth );
			When_Growing( 0 ); // triggers elements
			Assert_BonusElements( elements );
		}

		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,2)]
		[InlineDataAttribute(3,2)]
		[InlineDataAttribute(4,3)]
		[InlineDataAttribute(5,4)]
		[InlineDataAttribute(6,5)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount ){
			// card:	1 2 2 3 4 5
			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_CardTrackIs( expectedCardPlayCount );
		}


		#region helpers

		void Given_IslandIsABC() {
			// Given: 3-board island
			gameState.Island = new Island( BoardA, BoardB, BoardC );
		}

		#endregion

	}

}
