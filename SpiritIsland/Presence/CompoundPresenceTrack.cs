using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public class CompoundPresenceTrack : IPresenceTrack {

		public CompoundPresenceTrack(params IPresenceTrack[] parts ) {
			this.parts = parts;
		}

		readonly IPresenceTrack[] parts;

		public IEnumerable<Track> RemovableOptions => parts.SelectMany(p=>p.RemovableOptions);

		public IEnumerable<Track> ReturnableOptions => parts.SelectMany(p=>p.ReturnableOptions);

		public IEnumerable<Track> Revealed => parts.SelectMany(p=>p.Revealed);

		public IReadOnlyCollection<Track> Slots => parts.SelectMany(p=>p.Slots).ToArray();

		public void AddElements( ElementCounts elements ) {
			foreach(var part in parts)
				part.AddElements( elements );
		}

		public bool Remove( Track track ) {
			bool revealed = parts.Any(part=>part.Remove(track));
			if(revealed)
				TrackRevealed?.Invoke(track);
			return revealed;
		}
		public event Action<Track> TrackRevealed;


		public bool Return( Track track ) {
			return parts.Any(part=>part.Return(track));
		}

		#region Memento

		public IMemento<IPresenceTrack> SaveToMemento() => new Memento( this );
		public void LoadFrom( IMemento<IPresenceTrack> memento ) => ((Memento)memento).Restore( this );

		protected class Memento : IMemento<IPresenceTrack> {
			public Memento( CompoundPresenceTrack src ) { 
				parts = src.parts.Select(s=>s.SaveToMemento()).ToArray();
			}
			public void Restore( IPresenceTrack src ) {
				var compound = (CompoundPresenceTrack)src;
				for(int i=0;i<parts.Length;++i)
					compound.parts[i].LoadFrom(parts[i]);
			}
			readonly IMemento<IPresenceTrack>[] parts;
			
		}

		#endregion

	}

}