
using SpiritIsland.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	/*

	=========================================================
	A Spread of Rampant Green

	* +1 presense to jungle or wetland (Always do this + one of the following)
	* reclaim, +1 power card
	* +1 presense range 1, play +1 extra card this turn
	* +1 power card, +3 energy

	0 1 plant 2 2 plant 3
	1 1 2 2 3 4

	Innate-1:  Creepers Tear Into Mortar  => slow, 0,any
	1 moon 2 plant   1 damange to 1 town or city
	2 moon 3 plant   repeat this power
	3 moon 4 plant   repeat this power again

	Innate-2:   All-Enveloping Green => fast, within 1 of sacred, any
	1 water 3 plant  defend 2
	2 water 4 plant  instead, defend 4
	3 water 1 rock 5 plant  also, remove 1 blight

	Special Rules - Choke the land with green - whenever invaders would ravage or build in a land with your sacred site, you may prevent it by destroying one of your presense in that land
		* Option > At growth time, create a pre-ravaging event for every land

	Special Rules - Steady regeneration - when adding presense to the board via growth, yo may optionally use your destroyed presense.
	// If the island is helathy, do so freely.  If the island is blighted, doing so costs 1 energy per destroyed presense you add.

	Infinite vitality
	The trees and stones speak of war
	---
	drift down into slumber
	gift of living energy
	lure of the unknown
	enticing spender

*/

	public class RampantGreen : Spirit {

		public const string Name = "A Spread of Rampant Green";

		public override string Text => Name;

		public RampantGreen():base(
			new Track[] { Track.Energy0, Track.Energy1, Track.PlantEnergy, Track.Energy2, Track.Energy2, Track.PlantEnergy, Track.Energy3 },
			new Track[] { Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4 },
			PowerCard.For<FieldsChokedWithGrowth>(),
			PowerCard.For<GiftOfProliferation>(),
			PowerCard.For<OvergrowInANight>(),
			PowerCard.For<StemTheFlowOfFreshWater>()
		) {
			// Special rules: steady regeneration
			this.Presence.CanPlaceDestroyedPresence = true; // !! leaky abstractions

			var placeOnWetlandOrJungle = new PlacePresence(2, Filter.JungleOrWetland, "W / J");

			GrowthOptions = new GrowthOption[]{
				// reclaim, +1 power card
				new GrowthOption(
					placeOnWetlandOrJungle,
					new ReclaimAll(), 
					new DrawPowerCard(1)
				),
				// +1 presense range 1, play +1 extra card this turn
				new GrowthOption(
					placeOnWetlandOrJungle,
					new PlacePresence(1),
					new PlayExtraCardThisTurn()
				),
				// +1 power card, +3 energy
				new GrowthOption(
					placeOnWetlandOrJungle,
					new GainEnergy(3), 
					new DrawPowerCard()
				),
			};

			this.InnatePowers = new InnatePower[] {
				InnatePower.For<CreepersTearIntoMortar>(),
				InnatePower.For<AllEnvelopingGreen>(),
			};

		}

		public override int NumberOfCardsPlayablePerTurn => base.NumberOfCardsPlayablePerTurn + tempCardBoost;

		public override void PurchaseAvailableCards( params PowerCard[] cards ) {
			base.PurchaseAvailableCards( cards );
			tempCardBoost = 0;
		}
		public int tempCardBoost = 0;

		public override void Initialize( Board board, GameState gs ) {
			base.Initialize(board,gs);

			// Setup: 1 in the highest numbered wetland and 1 in the jungle without any dahan
			Presence.PlaceOn( board.Spaces.Reverse().First(x=>x.Terrain==Terrain.Wetland) );
			Presence.PlaceOn( board.Spaces.Single(x=>x.Terrain==Terrain.Jungle && gs.GetDahanOnSpace(x)==0) );

			gs.PreRavaging.Handlers.Add( ChokeTheLandWithGreen_Ravage );
			gs.PreBuilding.Handlers.Add( ChokeTheLandWithGreen_Build );

		}

		async Task ChokeTheLandWithGreen_Ravage( GameState gs, Space[] ravageSpaces ) {
			var stopped = await ChokeTheLandWithGreen( gs, ravageSpaces, "ravage" );
			gs.SkipRavage( stopped );
		}

		async Task ChokeTheLandWithGreen_Build( GameState gs, Space[] ravageSpaces ) {
			var stopped = await ChokeTheLandWithGreen( gs, ravageSpaces, "build" );
			gs.SkipBuild( stopped );
		}

		async Task<Space[]> ChokeTheLandWithGreen( GameState gs, Space[] ravageSpaces, string actionText ) {

			var stoppable = ravageSpaces.Intersect( SacredSites ).ToList();
			bool costs1 = gs.BlightCard.IslandIsBlighted;
			int maxStoppable = costs1 ? Energy : int.MaxValue;
			var eng = new ActionEngine( this, gs );
			var skipped = new List<Space>();
			while(maxStoppable > 0 && stoppable.Count > 0) {
				var stop = await eng.SelectSpace( $"Stop {actionText} by destroying 1 presence", stoppable.ToArray(), true );
				if(stop == null) break;
				Presence.Destroy( stop );
				skipped.Add( stop );
				--maxStoppable;
				if(costs1) --Energy;
			}
			return skipped.ToArray();
		}

		public override void AddActionFactory( IActionFactory actionFactory ) {

			if(actionFactory is DrawPowerCard) {
				var newCard = PowerProgression[0];
				this.RegisterNewCard( newCard );
				PowerProgression.RemoveAt( 0 );
				if(newCard.PowerType == PowerType.Major)
					base.AddActionFactory( new ForgetPowerCard() );
			} else
				base.AddActionFactory( actionFactory );
		}

		readonly List<PowerCard> PowerProgression = new List<PowerCard>{
			PowerCard.For<TheTreesAndStonesSpeakOfWar>(),
			PowerCard.For<InfiniteVitality>(),
			PowerCard.For<DriftDownIntoSlumber>(),
			PowerCard.For<GiftOfLivingEnergy>(),  // MAJOR?
			PowerCard.For<LureOfTheUnknown>(),
			PowerCard.For<EnticingSplendor>(),
		};

	}

	public class InfiniteVitality { } // major
	public class DriftDownIntoSlumber { }
	public class GiftOfLivingEnergy { }
	public class LureOfTheUnknown { }
	public class EnticingSplendor { }

}
