namespace SpiritIsland;

#nullable enable
public class Incarna( Spirit _spirit, string _abrev, Img _notEmpowered, Img _empowered ) 
	: IToken
	, IAppearInSpaceAbreviation
	, ITokenClass
	, ITrackMySpaces
{

	static readonly FakeSpace NullSpace = new FakeSpace( "Incarna-Null-Space" ); // 1 null space for all Incarna

	public Spirit Self { get; } = _spirit;

	public SpaceState Space => _spaceCounts.Keys
		.Where( ss => SpiritIsland.Space.Exists(ss.Space) )
		.SingleOrDefault() ?? NullSpace.ScopeTokens;

	public bool IsPlaced => _spaceCounts.Count != 0;

	public SpaceToken AsSpaceToken() => this.On(Space); // Assumes Space is not null.

	public bool Empowered { get; set; }

	public Img Img => Empowered ? _empowered: _notEmpowered;

	public string Text => SpaceAbreviation;

	public string SpaceAbreviation => _abrev + (Empowered ? "+" : "-");

	public ITokenClass Class => this;


	#region IEntityClass properties
	string ITag.Label => $"{Self.Text} Incarna";

	// Class & Token method
	public bool HasTag(ITag tag) 
		=> tag == this || tag == TokenCategory.Incarna // Is: This Incarna token or any Incarna
		|| ((ITokenClass)Self.Presence).HasTag(tag);   // Acts as Spirit Presence and/or belongs to this spirit.

	#endregion

	// !!! check that SpiritActions that Move/Add Invarna use this.
	public async Task MoveTo( Space destination, bool allowAdd ) {
		if(IsPlaced)
			await this.MoveAsync(Space,destination);
		else if(allowAdd)
			await destination.ScopeTokens.AddAsync( this, 1 );
	}

	void ITrackMySpaces.Clear() {
		_spaceCounts.Clear();
	}
	void ITrackMySpaces.TrackAdjust(SpaceState space, int delta) {
		_spaceCounts[space] += delta;
		if (1 < _spaceCounts.Count)
			throw new Exception("Incarna is in 2 places at the same time!");
	}
	//void ITrackMySpaces.TrackAdjust( Space space, int delta ) {
	//	_spaceCounts[space] += delta;
	//	if(1 < _spaceCounts.Count)
	//		throw new Exception("Incarna is in 2 places at the same time!");
	//}

	readonly CountDictionary<SpaceState> _spaceCounts = [];
}
