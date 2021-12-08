using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	class SerpentPresence : SpiritPresence {


		readonly static Track fakeEarth = new Track("earth energy" ) {
			Icon = new IconDescriptor { Text = "><" }
		};

		public SerpentPresence():base(
			new PresenceTrack( Track.Energy1, Track.FireEnergy, Track.AnyEnergy, Track.Reclaim1Energy, Track.EarthEnergy, Track.Energy6, Track.AnyEnergy, Track.MkEnergy(12) ),
			new PresenceTrack( Track.Card1,   Track.MkCard(Element.Moon), Track.Card2, Track.MkCard( Element.Water ), fakeEarth, Track.Card4,   Track.Card5Reclaim1 )
		){
		}

		public override IEnumerable<Track> RevealOptions { get {
			if(MaxPresenceOnBoard == Placed.Count ) yield break;

			var energyNext = Energy.RevealOptions.FirstOrDefault();
			if( energyNext != null && (energyNext != Track.EarthEnergy || CardPlays.Revealed.Count() == 4 ) )
				yield return energyNext;

			var cardNext = CardPlays.RevealOptions.FirstOrDefault();
			if( cardNext != null && cardNext != fakeEarth )  // don't let them select the fake earth
				yield return cardNext;

		} }

		public List<Spirit> AbsorbedPresences = new List<Spirit>(); // don't let it grow pas 6 elements

		public int MaxPresenceOnBoard => new int[]{5,7,8,10,11,12,13 }[AbsorbedPresences.Count];

		protected override async Task RevealTrack( Track track, GameState gs ) {
			await base.RevealTrack( track, gs );
			// When top row clears earth energy, clear the lower one too
			if( track == Track.EarthEnergy )
				await base.RevealTrack( fakeEarth, gs );
		}
	}

}
