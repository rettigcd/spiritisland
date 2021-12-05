using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SpiritPresence : IKnowSpiritLocations {

		#region constructors

		public SpiritPresence( IPresenceTrack energy, IPresenceTrack cardPlays ) {
			Energy = energy;
			CardPlays = cardPlays;

			Energy.TrackRevealed.Add( OnRevealed );
			CardPlays.TrackRevealed.Add( OnRevealed );

			foreach(var r in Energy.Revealed) CheckEnergyAndCardPlays( r);
			foreach(var r in CardPlays.Revealed) CheckEnergyAndCardPlays( r);
		}

		#endregion

		#region Tracks / Board

		public virtual IEnumerable<Track> RevealOptions 
			=> Energy.RevealOptions.Union( CardPlays.RevealOptions );

		public IEnumerable<Track> GetReturnableTrackOptions()
			=> Energy.ReturnableOptions.Union( CardPlays.ReturnableOptions );

		public IReadOnlyCollection<Track> GetEnergyTrackStatus() => Energy.Slots;

		public IReadOnlyCollection<Track> GetCardPlayTrackStatus() => CardPlays.Slots;

		public int EnergyPerTurn { get; private set; }

		// ! These 2 tracks are only public so they can be accessed from Tests.
		// ! Not accessed from production code
		public IPresenceTrack Energy { get; }
		public IPresenceTrack CardPlays { get; }

		public int Destroyed { get; set; }

		public void RemoveDestroyed(int count ) {
			if(count>Destroyed) throw new ArgumentOutOfRangeException(nameof(count));
			Destroyed -= count;
		}

		#endregion

		#region Readonly 

		public IEnumerable<Space> Spaces => placed.Distinct();

		public int CountOn( Space space ) => placed.Count( p => p == space );

		public bool IsOn( Space space ) => placed.Contains( space );

		public IEnumerable<IActionFactory> RevealedActions 
			=> CardPlays.Revealed
				.Union(Energy.Revealed)
				.Select(x => x.Action)
				.Where(x => x != null);

		public virtual IEnumerable<Space> SacredSites => Placed
			.GroupBy(x=>x)
			.Where(grp=>grp.Count()>1)
			.Select(grp=>grp.Key);

		#endregion

		#region Game-Play things you can do with presence

		public virtual async Task Place( IOption from, Space to, GameState gs ) {
			// from
			if(from is Track track) {
				await RevealTrack( track, gs );
			} else if(from is Space space) {
				if( Spaces.Contains(space) )
					RemoveFrom_NoCheck( space, gs );
				else
					throw new ArgumentException( "Can't pull from island space:" + from.ToString() );
			}

			// To
			PlaceOn( to, gs );
		}

		protected virtual async Task RevealTrack( Track track, GameState gs ) {
			if(track == Track.Destroyed && Destroyed > 0)
				--Destroyed;
			else if( !( await Energy.Reveal(track,gs) || await CardPlays.Reveal(track,gs) ) )
				throw new ArgumentException( "Can't pull from track:" + track.ToString() );
		}

		Task OnRevealed(GameState gs, Track track) {
			CheckEnergyAndCardPlays( track );
			return TrackRevealed.InvokeAsync( gs, track );
		}

		private void CheckEnergyAndCardPlays( Track track ) {
			if(track.Energy.HasValue && EnergyPerTurn < track.Energy.Value)
				EnergyPerTurn = track.Energy.Value;
			if(track.CardPlay.HasValue && CardPlayCount < track.CardPlay.Value)
				CardPlayCount = track.CardPlay.Value;
		}

		public virtual Task ReturnDestroyedToTrack( Track dst, GameState gs ) {

			// src / from
			if(Destroyed <=0 ) throw new InvalidOperationException("There is no Destoryed presence to return.");
			--Destroyed;

			// To
			return (Energy.Return(dst) || CardPlays.Return(dst))
				? Task.CompletedTask
				: throw new ArgumentException("Unable to find location to restore presence");
		}

		public void Move( Space from, Space to, GameState gs ) {
			RemoveFrom_NoCheck( from, gs );
			PlaceOn( to, gs );
		}

		public async Task Destroy( Space space, GameState gs, Cause cause ) {
			await DestroyBehavior.DestroyPresenceApi(this,space,gs,cause);
			CheckIfSpiritIsDestroyted();
		}

		void CheckIfSpiritIsDestroyted() {
			if(Placed.Count == 0 && stasis.Count == 0 )
				GameOverException.Lost( "Spirit is Destroyed" ); // !! if we had access to the Spirit here, we could say who it was.
		}

		public IDestroyPresenceBehavour DestroyBehavior = new DefaultDestroyBehavior(); // replaceable / plugable

		public class DefaultDestroyBehavior : IDestroyPresenceBehavour {
			public virtual Task DestroyPresenceApi(SpiritPresence presence, Space space, GameState gs, Cause cause ) {
				presence.RemoveFrom_NoCheck( space, gs );
				++presence.Destroyed;
				return Task.CompletedTask;
			}

		}

		#endregion

		#region Setup / Adjust

		public virtual void PlaceOn(Space space, GameState _) => placed.Add(space);

		/// <remarks>public so we can remove it for Replacing with Beast and advanced spirit strangness</remarks>
		// !!! This is called for 2 different reasons...
		// (1) To move presence to another location on the board - no End-of-Game check is necessary
		// (2) Presence is replaced with something else. End-of-Game check IS necessary.
		// Also - if we have presence in Stasis, then removing 2nd to last presence will INCORRECTLY trigger loss.
		protected virtual void RemoveFrom_NoCheck( Space space, GameState _ ) => placed.Remove( space );

		public void RemoveFrom( Space space, GameState gs ) {
			RemoveFrom_NoCheck( space, gs );
			CheckIfSpiritIsDestroyted();
		}

		public void PutInStasis( Space space, GameState gs ) {
			while( IsOn(space)) {
				RemoveFrom_NoCheck( space, gs );
				stasis.Add(space);
			}
		}

		public void ReleaseFromStasis( Space space, GameState gs ) {
			while( stasis.Contains(space)) {
				stasis.Remove(space);
				PlaceOn(space, gs);
			}
		}
		readonly List<Space> stasis = new List<Space>();

		#endregion

		#region Per Turn stuff

		public int CardPlayCount { get; private set; }

		public ElementCounts AddElements( ElementCounts elements=null ) {
			if(elements==null) elements = new ElementCounts();
			Energy.AddElements( elements );
			CardPlays.AddElements( elements);
			return elements;
		}

		#endregion

		/// <summary>
		/// Specifies if the the given space is valid.
		/// </summary>
		public Func<Space,bool> IsValid = DefaultIsValid; // override for Lure, Ocean, Volcano

		static bool DefaultIsValid(Space space) => space.Terrain != Terrain.Ocean;

		public IReadOnlyCollection<Space> Placed => placed.AsReadOnly();

		public DualAsyncEvent<Track> TrackRevealed { get; } = new DualAsyncEvent<Track>();

		readonly List<Space> placed = new List<Space>();

		#region Memento

		// Revealed Count + Placed.
		public virtual IMemento<SpiritPresence> SaveToMemento() => new Memento(this);
		public virtual void LoadFrom( IMemento<SpiritPresence> memento ) => ((Memento)memento).Restore(this);

		protected class Memento : IMemento<SpiritPresence> {
			public Memento(SpiritPresence src) {
				placed = src.placed.ToArray();
				energy = src.Energy.SaveToMemento();
				cardPlays = src.CardPlays.SaveToMemento();
				destroyed = src.Destroyed;
			}
			public void Restore(SpiritPresence src ) {
				src.placed.Clear(); src.placed.AddRange( placed );
				src.Energy.LoadFrom(energy);
				src.CardPlays.LoadFrom(cardPlays);
				src.Destroyed = destroyed;
			}
			readonly Space[] placed;
			readonly IMemento<IPresenceTrack> energy;
			readonly IMemento<IPresenceTrack> cardPlays;
			readonly int destroyed;
		}

		#endregion
	}

}