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

	public void AddElementsTo( CountDictionary<Element> elements ) {
		foreach(var part in parts)
			part.AddElementsTo( elements );
	}

	public async Task<bool> RevealAsync( Track track ){
		foreach(var part in parts) {
			if( await part.RevealAsync( track ) ) {
				await OnTrackRevealed(track);
				return true;
			}
		}
		return false;
	}

	async Task OnTrackRevealed(Track track){
		if(TrackRevealedAsync is not null)
			await TrackRevealedAsync( new TrackRevealedArgs(track));
	}

	public event Func<TrackRevealedArgs,Task> TrackRevealedAsync;

	public bool Return( Track track ) {
		return parts.Any(part=>part.Return(track));
	}

	#region Memento

	public object Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	protected class MyMemento {
		public MyMemento( CompoundPresenceTrack src ) { 
			parts = src.parts.Select(s=>s.Memento).ToArray();
		}
		public void Restore( IPresenceTrack src ) {
			var compound = (CompoundPresenceTrack)src;
			for(int i=0;i<parts.Length;++i)
				compound.parts[i].Memento = parts[i];
		}
		readonly object[] parts;
			
	}

	#endregion

}