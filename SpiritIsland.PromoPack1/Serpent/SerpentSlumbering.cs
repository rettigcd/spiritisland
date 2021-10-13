using System.Collections.Generic;

namespace SpiritIsland.PromoPack1 {

	// Presence Tracks
	// Innates
	// Power CArds

	// !!! Card 5 needs Reclaim One

	public class SerpentSlumbering : Spirit {

		public SerpentSlumbering() :base (
			new SerpentPresence()
			,PowerCard.For<ElementalAegis>()
			,PowerCard.For<AbsorbEssence>()
			,PowerCard.For<GiftOfFlowingPower>()
			,PowerCard.For<GiftOfThePrimordialDeeps>()	
		) {
			InnatePowers = new InnatePower[] {
				InnatePower.For<SerpentWakesInPower>(),
				InnatePower.For<SerpentRousesInAnger>()
			};

			GrowthOptions = new GrowthOption[] {
				new GrowthOption( new ReclaimAll(), new MovePresence() ),
				new GrowthOption( new DrawPowerCard(), new GainEnergy(1) ),
				new GrowthOption( new GainEnergy(4) ),
				new GrowthOption( new PlacePresence(3,Target.NoBlight) ),
			};
			growthOptionSelectionCount = 2;

		}

		public const string Name = "Serpent Slumbering Beneath the Island";

		public override string Text => Name;

		public override string SpecialRules => "Deep Slumber - You start off limited to 5 presence on the tisland.  Raise this with your 'Absorb Essence' Power Card.  Each use covers the lowest revealed number; your presence limit is the lowest uncovered number.";

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Setup: put 1 presence on #5
			Presence.PlaceOn(board[5]);
		}

	}

	class SerpentPresence : SpiritPresence {

		public SerpentPresence():base(
			new Track[]{ Track.Energy1, Track.FireEnergy, Track.AnyEnergy,    Track.Reclaim1Energy, Track.EarthEnergy, Track.Energy6, Track.AnyEnergy, Track.MkEnergy(12) },
			new Track[]{ Track.Card1,   Track.MoonEnergy, Track.Card2,        Track.WaterEnergy, Track.EarthEnergy, Track.Card4,   Track.Card5Reclaim1 }
		){
		}

		public override IEnumerable<Track> GetPlaceableTrackOptions() {
			if(Placed.Count < MaxPresenceOnBoard) {

				if(Energy.RevealedCount <= 4 || Energy.HasMore && 4 < CardPlays.RevealedCount ) 
					yield return Energy.Next;

				if(CardPlays.RevealedCount <= 4 || CardPlays.HasMore && 4 < Energy.RevealedCount ) 
					yield return CardPlays.Next;
			}
		}

		public List<Spirit> AbsorbedPresences = new List<Spirit>(); // don't let it grow pas 6 elements

		public int MaxPresenceOnBoard => new int[]{5,7,8,10,11,12,13 }[AbsorbedPresences.Count];

	}

}
