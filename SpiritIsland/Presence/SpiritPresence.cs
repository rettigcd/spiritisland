﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SpiritPresence {

		public SpiritPresence( Track[] energy, Track[] cardPlays ){
			Energy = new PresenceTrack(energy);
			CardPlays = new PresenceTrack(cardPlays);
		}

		public SpiritPresence( PresenceTrack energy, PresenceTrack cardPlays ) {
			Energy = energy;
			CardPlays = cardPlays;
		}


		#region Tracks / Board

		public virtual IEnumerable<Track> GetPlaceableTrackOptions() {
			if(Energy.HasMore) yield return Energy.Next;
			if(CardPlays.HasMore) yield return CardPlays.Next;
		}

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
				RemoveFromTrack( track );
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

		protected virtual void RemoveFromTrack( Track track ) {
			if(track == Track.Destroyed && Destroyed > 0)
				--Destroyed;
			else if(Energy.HasMore && track == Energy.Next)
				Energy.RevealedCount++;
			else if(CardPlays.HasMore && track == CardPlays.Next)
				CardPlays.RevealedCount++;
			else
				throw new ArgumentException( "Can't pull from track:" + track.ToString() );
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

		public void AddElements( CountDictionary<Element> elements ) {
			Energy.AddElements( elements );
			CardPlays.AddElements( elements);
		}

		readonly List<Space> placed = new List<Space>();

		// Revealed Count + Placed.
		public virtual IMemento<SpiritPresence> SaveToMemento() => new Memento(this);
		public virtual void LoadFrom( IMemento<SpiritPresence> memento ) => ((Memento)memento).Restore(this);

		protected class Memento : IMemento<SpiritPresence> {
			public Memento(SpiritPresence src) {
				placed = src.placed.ToArray();
				revealedEnergy = src.Energy.RevealedCount;
				revealedCardPlays = src.CardPlays.RevealedCount;
				destroyed = src.Destroyed;
			}
			public void Restore(SpiritPresence src ) {
				src.placed.Clear(); src.placed.AddRange( placed );
				src.Energy.RevealedCount = revealedEnergy;
				src.CardPlays.RevealedCount = revealedCardPlays;
				src.Destroyed = destroyed;
			}
			readonly Space[] placed;
			readonly int revealedEnergy;
			readonly int revealedCardPlays;
			readonly int destroyed;
		}
	}

}