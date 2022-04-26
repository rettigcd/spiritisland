namespace SpiritIsland;
public class CompoundPresenceTrack : IPresenceTrack {

	public CompoundPresenceTrack(params IPresenceTrack[] parts ) {
		this.parts = parts;
	}

	readonly IPresenceTrack[] parts;

	public IEnumerable<Track> RevealOptions => parts.SelectMany(p=>p.RevealOptions);

	public IEnumerable<Track> ReturnableOptions => parts.SelectMany(p=>p.ReturnableOptions);

	public IEnumerable<Track> Revealed => parts.SelectMany(p=>p.Revealed);

	public IReadOnlyCollection<Track> Slots => parts.SelectMany(p=>p.Slots).ToArray();

	public void AddElements( ElementCounts elements ) {
		foreach(var part in parts)
			part.AddElements( elements );
	}

	public async Task<bool> Reveal( Track track, GameState gs ) {
		foreach(var part in parts) {
			if(await part.Reveal( track, gs )) {
				await TrackRevealed.InvokeAsync( new TrackRevealedArgs( track, gs ) );
				return true;
			}
		}
		return false;
	}

	public AsyncEvent<TrackRevealedArgs> TrackRevealed { get; } = new AsyncEvent<TrackRevealedArgs>();

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