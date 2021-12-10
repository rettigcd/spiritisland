using SpiritIsland;
using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.OceanNS {

	public class Ocean_GrowthTests : GrowthTests {

		static Spirit InitSpirit() {
			return new Ocean {
				CardDrawer = new PowerProgression(
					PowerCard.For<VeilTheNightsHunt>(),
					PowerCard.For<ReachingGrasp>(),
					PowerCard.For<Drought>(),
					PowerCard.For<ElementalBoon>()
				)
			};
		}

		public Ocean_GrowthTests():base( InitSpirit() ) {}

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

			spirit.Grow(spirit.Growth.Options[0]);

			// since options are move source, key on that
			var moveBySrc = select.Split(',')
				.Where(x=>!string.IsNullOrEmpty(x))
				.Select(s=>s.Split('>'))
				.ToDictionary(a=>a[0],a=>a[1]);

			GatherPresenceIntoOcean gather = spirit.GetAvailableActions(Phase.Growth).OfType<GatherPresenceIntoOcean>().SingleOrDefault();

			if(gather != null){
				_ = gather.ActivateAsync( new SpiritGameStateCtx( spirit, gameState, Cause.Growth ) );
				while(!spirit.Action.IsResolved){
					var source = spirit.Action.GetCurrent().Options.Single(x=>moveBySrc.ContainsKey(x.Text));
					spirit.Action.Choose(source);
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
		}

		[Fact]
		public void ReclaimGather_NonGatherParts() {
			// reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy

			Given_HalfOfPowercardsPlayed();
			_ = When_Growing( 0 );

			User.Growth_ReclaimsAll();
			User.Growth_DrawsPowerCard();
			User.Growth_GainsEnergy();
			User.GathersPresenceIntoOcean();

			Assert_AllCardsAvailableToPlay( 4 + 1 );
			Assert_GainsFirstPowerProgressionCard();
			Assert_HasEnergy( 2 );
		}

		[Fact]
		public void TwoPresenceInOceans() {
			// +1 presence range any ocean, +1 presense in any ociean, +1 energy

			// Given: island has 2 boards, hence 2 oceans
			gameState.Island = new Island( BoardA, BoardB );

			_ = When_Growing( 1 );

			User.Growth_GainsEnergy();
			User.PlacesPresenceInOcean( "PlaceInOcean,(PlaceInOcean)", "(moon energy),2 cardplay,Take Presence from Board", "(A0),B0" );
			User.PlacesPresenceInOcean( "PlaceInOcean", "(water energy),2 cardplay,Take Presence from Board", "A0,(B0)" );

			Assert_HasEnergy( 1 );
		}

		[Theory]
		[InlineData("A0","A1;A2;A3","A1A2")]
		public void PowerPlaceAndPush( string starting, string placeOptions, string ending ) {
			// gain power card
			// push 1 presense from each ocean
			// add presense on costal land range 1
			gameState.Island = new Island( BoardA, BoardB, BoardC );
			Given_HasPresence( starting );

			_ = When_Growing( 2 );

			User.Growth_PlacesEnergyPresence( placeOptions );
			User.Growth_DrawsPowerCard();
			User.PushesPresenceFromOcean("A1,(A2),A3");

			Assert_GainsFirstPowerProgressionCard();
			Assert_BoardPresenceIs( ending );
		}

		void Assert_GainsFirstPowerProgressionCard() {
			Assert_HasCardAvailable( "Veil the Night's Hunt" );
		}

		[Trait("Presence","EnergyTrack")]
		[Theory]
		[InlineDataAttribute(1,0,"")]
		[InlineDataAttribute(2,0,"M")]
		[InlineDataAttribute(3,0,"MW")]
		[InlineDataAttribute(4,1,"MW")]
		[InlineDataAttribute(5,1,"MWE")]
		[InlineDataAttribute(6,1,"MWEW")]
		[InlineDataAttribute(7,2,"MWEW")]
		public async Task EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			// energy: 0 moon water 1 earth water 2
			spirit.Presence.Energy.SetRevealedCount( revealedSpaces );
			spirit.InitElementsFromPresence();
			Assert_EnergyTrackIs( expectedEnergyGrowth );
			await spirit.ApplyRevealedPresenceTracks(null);
			Assert_BonusElements( elements );
		}

		[Trait("Presence","CardTrack")]
		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,2)]
		[InlineDataAttribute(3,2)]
		[InlineDataAttribute(4,3)]
		[InlineDataAttribute(5,4)]
		[InlineDataAttribute(6,5)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount ){
			// card:	1 2 2 3 4 5
			spirit.Presence.CardPlays.SetRevealedCount( revealedSpaces );
			Assert_CardTrackIs( expectedCardPlayCount );
		}

		void Given_IslandIsABC() {
			// Given: 3-board island
			gameState.Island = new Island( BoardA, BoardB, BoardC );
		}

	}

}
