namespace SpiritIsland;
public class CompoundPresenceTrack( params IPresenceTrack[] parts ) 
	: IPresenceTrack
{
	readonly IPresenceTrack[] _parts = parts;

	public IEnumerable<Track> RevealOptions => _parts.SelectMany(p=>p.RevealOptions);

	public IEnumerable<Track> ReturnableOptions => _parts.SelectMany(p=>p.ReturnableOptions);

	public IEnumerable<Track> Revealed => _parts.SelectMany(p=>p.Revealed);

	public IReadOnlyCollection<Track> Slots => _parts.SelectMany(p=>p.Slots).ToArray();

	public void AddElementsTo( CountDictionary<Element> elements ) {
		foreach(var part in _parts)
			part.AddElementsTo( elements );
	}

	public async Task<bool> RevealAsync( Track track ){
		foreach(var part in _parts) {
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

	public event Func<TrackRevealedArgs,Task>? TrackRevealedAsync;

	public bool Return( Track track ) {
		return _parts.Any(part=>part.Return(track));
	}

	#region Memento

	public object Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	protected class MyMemento( CompoundPresenceTrack src ) {
		public void Restore( IPresenceTrack src ) {
			var compound = (CompoundPresenceTrack)src;
			for(int i=0;i<parts.Length;++i)
				compound._parts[i].Memento = parts[i];
		}
		readonly object[] parts = src._parts.Select( s => s.Memento ).ToArray();

	}

	#endregion

	#region Json

	/// <summary> [ part0Json, part1Json, ... ], recursively - which part revealed how much matters, not
	/// just the combined total, so each part restores itself onto the already-constructed instance
	/// (same shape as PresenceTrack.RestoreFromJson) rather than a single combined count. </summary>
	JsonNode IPresenceTrack.ToJson() => new JsonArray( _parts.Select( p => p.ToJson() ).ToArray() );

	void IPresenceTrack.RestoreFromJson( JsonNode json ) {
		var array = (JsonArray)json;
		for( int i = 0; i < _parts.Length; ++i )
			_parts[i].RestoreFromJson( array[i]! );
	}

	#endregion

}