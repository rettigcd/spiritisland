using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	class SerpentPresence : SpiritPresence {


		readonly static Track fakeEarth = new Track("earth energy");

		public SerpentPresence():base(
			new Track[]{ Track.Energy1, Track.FireEnergy, Track.AnyEnergy,    Track.Reclaim1Energy, Track.EarthEnergy, Track.Energy6, Track.AnyEnergy, Track.MkEnergy(12) },
			new Track[]{ Track.Card1,   Track.MoonEnergy, Track.Card2,        Track.WaterEnergy,    fakeEarth, Track.Card4,   Track.Card5Reclaim1 }
		){
		}

		public override IEnumerable<Track> GetPlaceableTrackOptions() {
			if(MaxPresenceOnBoard == Placed.Count ) yield break;

			if( Energy.HasMore && (Energy.Next != Track.EarthEnergy || CardPlays.RevealedCount == 4 ) )
				yield return Energy.Next;

			if( CardPlays.HasMore && CardPlays.Next != fakeEarth )  // don't let them select the fake earth
				yield return CardPlays.Next;

		}

		public List<Spirit> AbsorbedPresences = new List<Spirit>(); // don't let it grow pas 6 elements

		public int MaxPresenceOnBoard => new int[]{5,7,8,10,11,12,13 }[AbsorbedPresences.Count];

		protected override void RemoveFromTrack( Track track ) {
			base.RemoveFromTrack( track );
			// When top row clears earth energy, clear the lower one too
			if( track == Track.EarthEnergy )
				base.RemoveFromTrack( fakeEarth );
		}
	}

}
