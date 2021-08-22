using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public class MyPresence {

		public MyPresence( Track[] energy, Track[] cardPlays ){
			Energy = new PresenceTrack(energy);
			CardPlays = new PresenceTrack(cardPlays);
		}

		public MyPresence( PresenceTrack energy, PresenceTrack cardPlays ) {
			Energy = energy;
			CardPlays = cardPlays;
		}


		#region Tracks / Board

		public Track[] GetPlaceableFromTracks() {
			var tracks = new List<Track>();
			if(Energy.HasMore) tracks.Add( Energy.Next );
			if(CardPlays.HasMore) tracks.Add( CardPlays.Next );
			if(CanPlaceDestroyedPresence && Destroyed>0) tracks.Add(Track.Destroyed);
			return tracks.ToArray();
		}

		public bool CanPlaceDestroyedPresence = false;
		public PresenceTrack Energy { get; }
		public PresenceTrack CardPlays { get; }
		public int Destroyed { get; private set; }

		#endregion

		#region Board (readonly)

		public IEnumerable<Space> Spaces => placed.Distinct();

		public int CountOn( Space space ) => placed.Count( p => p == space );

		public bool IsOn( Space space ) => placed.Contains( space );

		#endregion

		#region Game-Play things you can do with presence

		public void PlaceFromBoard( Track from, Space to ) {
			// from
			if(from == Track.Destroyed)
				--Destroyed;
			else if(from == Energy.Next)
				Energy.RevealedCount++;
			else if(from == CardPlays.Next)
				CardPlays.RevealedCount++;
			else
				throw new ArgumentException( from.ToString() );

			// To
			PlaceOn( to );
		}

		public void Move( Space from, Space to ) {
			RemoveFrom( from );
			PlaceOn( to );
		}

		public void Destroy( Space space ) {
			RemoveFrom( space );
			++Destroyed;
		}

		#endregion

		#region Setup / Adjust
		public virtual void PlaceOn(Space space) => placed.Add(space);

		public virtual void RemoveFrom( Space space ) => placed.Remove( space );

		#endregion

		public IReadOnlyCollection<Space> Placed => placed.AsReadOnly();
		readonly List<Space> placed = new List<Space>();

	}

}