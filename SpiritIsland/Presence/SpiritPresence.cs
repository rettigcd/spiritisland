﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SpiritPresence : IKnowSpiritLocations {

//		readonly UniqueToken Token = new UniqueToken( "Presence",'p', Img.Icon_Presence, TokenCategory.Presence );

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

		public IReadOnlyCollection<Track> GetEnergyTrack() => Energy.Slots;

		public IReadOnlyCollection<Track> GetCardPlayTrack() => CardPlays.Slots;

		public int EnergyPerTurn { get; private set; }

		// ! These 2 tracks are only public so they can be accessed from Tests.
		// ! Not accessed from production code
		public IPresenceTrack Energy { get; }
		public IPresenceTrack CardPlays { get; }

		public int Destroyed { get; set; }

		public void RemoveDestroyed( int count ) {
			if(count>Destroyed) throw new ArgumentOutOfRangeException(nameof(count));
			Destroyed -= count;
		}

		#endregion

		#region Readonly 

		public IEnumerable<IActionFactory> RevealedActions 
			=> CardPlays.Revealed
				.Union(Energy.Revealed)
				.Select(x => x.Action)
				.Where(x => x != null);

		public virtual IEnumerable<Space> SacredSites => Placed
			.GroupBy(x=>x)
			.Where(grp=>grp.Count()>1)
			.Select(grp=>grp.Key);

		public bool IsSacredSite( Space space ) => SacredSites.Contains( space );

		#endregion

		#region Game-Play things you can do with presence

		public virtual async Task Place( IOption from, Space to, GameState gs ) {
			await TakeFrom( from, gs );
			await PlaceOn( to, gs );
		}

		public async Task TakeFrom( IOption from, GameState gs ) {
			if(from is Track track) {
				await RevealTrack( track, gs );
			} else if(from is Space space) {
				if(Spaces.Contains( space ))
					await RemoveFrom_NoCheck( space, gs );
				else
					throw new ArgumentException( "Can't pull from island space:" + from.ToString() );
			}
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

		public async Task Destroy( Space space, GameState gs, ActionType actionType ) {
			await DestroyBehavior.DestroyPresenceApi(this,space,gs, actionType); // !!!
			CheckIfSpiritIsDestroyted();
		}

		void CheckIfSpiritIsDestroyted() {
			if(Placed.Count == 0 && stasis.Count == 0 )
				GameOverException.Lost( "Spirit is Destroyed" ); // !! if we had access to the Spirit here, we could say who it was.
		}

		public IDestroyPresenceBehavour DestroyBehavior = new DefaultDestroyBehavior(); // replaceable / plugable

		public class DefaultDestroyBehavior : IDestroyPresenceBehavour {
			public virtual Task DestroyPresenceApi(SpiritPresence presence, Space space, GameState gs, ActionType actionType ) {
				presence.RemoveFrom_NoCheck( space, gs );
				++presence.Destroyed;
				return Task.CompletedTask;
			}

		}

		#endregion

		#region Setup / Adjust

		public async Task RemoveFrom( Space space, GameState gs ) {
			await RemoveFrom_NoCheck( space, gs );
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

		static bool DefaultIsValid(Space space) => !space.IsOcean;

		public DualAsyncEvent<Track> TrackRevealed { get; } = new DualAsyncEvent<Track>();

		public IReadOnlyCollection<Space> Placed => placed.AsReadOnly();

		public IEnumerable<Space> Spaces => placed.Distinct();

		public int CountOn( Space space ) => placed.Count( p => p == space );

		public bool IsOn( Space space ) => placed.Contains( space );

		public virtual Task PlaceOn(Space space, GameState gameState) { 
			placed.Add(space);
			return Task.CompletedTask;
			// return gameState.Tokens[space].Add(Token,1, AddReason.Added); // !!! this add reason might not be correct
		}

		/// <remarks>public so we can remove it for Replacing with Beast and advanced spirit strangness</remarks>
		// !!! This is called for 2 different reasons...
		// (1) To move presence to another location on the board - no End-of-Game check is necessary
		// (2) Presence is replaced with something else. End-of-Game check IS necessary.
		// Also - if we have presence in Stasis, then removing 2nd to last presence will INCORRECTLY trigger loss.
		protected virtual Task RemoveFrom_NoCheck( Space space, GameState gameState ) { 
			placed.Remove( space );
			return Task.CompletedTask;
//			return gameState.Tokens[space].Remove(Token,1); // !!! Do we event want to generate an event?
		}

		public void Adjust( Space space, int count ) {
			while( 0 < count) {
				placed.Add( space );
				--count;
			}
			while( count < 0 ) {
				placed.Remove( space );
				++count;
			}
		}

		readonly List<Space> placed = new List<Space>();
		readonly List<Space> stasis = new List<Space>();


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

	public enum ActionType {
		None,
		SpiritPower,    // One use of a Power;
		SpiritGrowth,   // A single Growth effect (nearly always "one icon");

		Invader,        // A Ravage, Build, or Explore in a single land;
		FearCard,       // Everything one Fear Card does (†);
		Event,          // Everything a Main/Token/Dahan Event does (†);
		BlightedIsland, // The effect of the Blighted Island card (†);
		Adversary,      //An Adversary's Escalation effect (†);
	}
}