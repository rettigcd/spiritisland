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

	public class ASpreadOfRampantGreen : Spirit {

		public const string Name = "A Spread of Rampant Green";

		public override string Text => Name;

		public override SpecialRule[] SpecialRules => new SpecialRule[] {
			new SpecialRule("Choke the land with green","Whenever invaders would ravage or build in a land with your sacred site, you may prevent it by destroying one of your presense in that land.")
		} ;

		public ASpreadOfRampantGreen():base(
			new RampantGreenPresence(),
			PowerCard.For<FieldsChokedWithGrowth>(),
			PowerCard.For<GiftOfProliferation>(),
			PowerCard.For<OvergrowInANight>(),
			PowerCard.For<StemTheFlowOfFreshWater>()
		) {
			// Special rules: steady regeneration

			static PlacePresence placeOnWetlandOrJungle() => new ( 2, Target.JungleOrWetland );

			Growth = new(
				// reclaim, +1 power card
				new GrowthOption(
					placeOnWetlandOrJungle(),
					new ReclaimAll(), 
					new DrawPowerCard(1)
				),
				// +1 presense range 1, play +1 extra card this turn
				new GrowthOption(
					placeOnWetlandOrJungle(),
					new PlacePresence(1),
					new PlayExtraCardThisTurn(1)
				),
				// +1 power card, +3 energy
				new GrowthOption(
					placeOnWetlandOrJungle(),
					new GainEnergy(3), 
					new DrawPowerCard()
				)
			);

			this.InnatePowers = new InnatePower[] {
				InnatePower.For<CreepersTearIntoMortar>(),
				InnatePower.For<AllEnvelopingGreen>(),
			};

		}

		protected override PowerProgression GetPowerProgression() =>
			new (
				PowerCard.For<DriftDownIntoSlumber>(),
				PowerCard.For<GiftOfLivingEnergy>(),
				PowerCard.For<TheTreesAndStonesSpeakOfWar>(), // major
				PowerCard.For<LureOfTheUnknown>(),
				PowerCard.For<InfiniteVitality>(), // major
				PowerCard.For<EnticingSplendor>()
			);


		protected override void InitializeInternal( Board board, GameState gs ) {

			// Setup: 1 in the highest numbered wetland and 1 in the jungle without any dahan
			Presence.PlaceOn( board.Spaces.Reverse().First(x=>x.Terrain==Terrain.Wetland), gs );
			Presence.PlaceOn( board.Spaces.Single(x=>x.Terrain==Terrain.Jungle && gs.DahanOn(x).Count==0), gs );

			gs.PreRavaging.ForGame.Add( ChokeTheLandWithGreen_Ravage );
			gs.PreBuilding.ForGame.Add( ChokeTheLandWithGreen_Build );

		}

		async Task ChokeTheLandWithGreen_Ravage( GameState gs, RavagingEventArgs args ) {
			var stopped = await ChokeTheLandWithGreen( gs, args.Spaces.ToArray(), "ravage" );
			foreach(var space in stopped)
				args.Skip1( space );
		}

		async Task ChokeTheLandWithGreen_Build( GameState gs, BuildingEventArgs args ) {
			Space[] buildSpaces = args.SpaceCounts.Keys.Where( k => args.SpaceCounts[k] > 0 ).ToArray();
			Space[] stopped = await ChokeTheLandWithGreen( gs, buildSpaces, "build" ); 
			foreach(var s in stopped)
				args.Skip1(s);
		}

		async Task<Space[]> ChokeTheLandWithGreen( GameState gs, Space[] spaces, string actionText ) {

			var stoppable = spaces.Intersect( Presence.SacredSites ).ToList();
			bool costs1 = gs.BlightCard.IslandIsBlighted;
			int maxStoppable = costs1 ? Energy : int.MaxValue;

			var skipped = new List<Space>();
			while(maxStoppable > 0 && stoppable.Count > 0) {
				var stop = await this.Action.Decision( new Select.Space( $"Stop {actionText} by destroying 1 presence", stoppable.ToArray(), Present.Done ) );
				if(stop == null) break;

				await Presence.Destroy( stop, gs, ActionType.Invader ); // it is the invader actions we are stopping

				skipped.Add( stop );
				stoppable.Remove( stop );
				--maxStoppable;

				if(costs1) --Energy;
			}
			return skipped.ToArray();
		}

	}

	public class RampantGreenPresence : SpiritPresence {

		public RampantGreenPresence() 
			: base(
				new PresenceTrack( Track.Energy0, Track.Energy1, Track.PlantEnergy, Track.Energy2, Track.Energy2, Track.PlantEnergy, Track.Energy3 ),
				new PresenceTrack( Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4 )
			) { }

		public override IEnumerable<Track> RevealOptions { get { 
			var options = base.RevealOptions.ToList();
			if( Destroyed>0 ) options.Add(Track.Destroyed);
			return options;
		} }

	}

}
