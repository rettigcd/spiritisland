using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

		public Track[] GetPlaceableTrackOptions() {
			var options = new List<Track>();
			if(Energy.HasMore) options.Add( Energy.Next );
			if(CardPlays.HasMore) options.Add( CardPlays.Next );
			if(CanPlaceDestroyedPresence && Destroyed>0) options.Add(Track.Destroyed);
			return options.ToArray();
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

		public virtual Task PlaceFromBoard( IOption from, Space to, GameState _ ) {
			// from
			if(from is Track track) {
				if(track == Track.Destroyed && Destroyed>0)
					--Destroyed;
				else if(Energy.HasMore && track == Energy.Next)
					Energy.RevealedCount++;
				else if(CardPlays.HasMore && track == CardPlays.Next)
					CardPlays.RevealedCount++;
				else
					throw new ArgumentException( "Can't pull from track:" + from.ToString() );
			} else if(from is Space space) {
				if( Spaces.Contains(space) )
					RemoveFrom( space );
				else
					throw new ArgumentException( "Can't pull from island space:" + from.ToString() );
			}

			// To
			PlaceOn( to );
			return Task.CompletedTask;
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

		/// <remarks>public so we can remove it for Replacing with Beast and advanced spirit strangness</remarks>
		public virtual void RemoveFrom( Space space ) => placed.Remove( space );

		#endregion

		public IReadOnlyCollection<Space> Placed => placed.AsReadOnly();
		readonly List<Space> placed = new List<Space>();

	}

}