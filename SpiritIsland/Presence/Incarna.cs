namespace SpiritIsland;

#nullable enable
public class Incarna : IToken, IAppearInSpaceAbreviation, ITokenClass, ITrackMySpaces {

	static readonly FakeSpace NullSpace = new FakeSpace( "Incarna-Null-Space" ); // 1 null space for all Incarna

	public Spirit Self { get; }

	public Incarna( Spirit spirit, string abrev, Img notEmpowered, Img empowered ) {
		Self = spirit;
		_abreviation = abrev;
		_notEmpowered = notEmpowered;
		_empowered = empowered;
	}

	public SpaceState Space => _spaceCounts.Keys
		.Where( SpiritIsland.Space.Exists )
		.Tokens()
		.SingleOrDefault() ?? NullSpace;

	public bool IsPlaced => _spaceCounts.Count != 0;

	public SpaceToken AsSpaceToken() => this.On(Space.Space); // Assumes Space is not null.

	public bool Empowered { get; set; }

	public Img Img => Empowered ? _empowered: _notEmpowered;

	public string Text => SpaceAbreviation;

	public string SpaceAbreviation => _abreviation + (Empowered ? "+" : "-");

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
			await destination.Tokens.AddAsync( this, 1 );
	}

	void ITrackMySpaces.Clear() {
		_spaceCounts.Clear();
	}
	void ITrackMySpaces.TrackAdjust( Space space, int delta ) {
		_spaceCounts[space] += delta;
		if(1 < _spaceCounts.Count)
			throw new Exception("Incarna is in 2 places at the same time!");
	}

	readonly CountDictionary<Space> _spaceCounts = new CountDictionary<Space>();
	readonly string _abreviation;
	readonly Img _notEmpowered;
	readonly Img _empowered;

}
