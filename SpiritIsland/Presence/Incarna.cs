namespace SpiritIsland;

#nullable enable
public class Incarna 
	: IIncarnaToken
	, ITokenClass
	, ITrackMySpaces
{

	public Spirit Self { get; }
	readonly string _abreviation;
	readonly Img _notEmpowered;
	readonly Img _empowered;

	public Incarna( Spirit spirit, string abrev, Img notEmpowered, Img empowered ) {
		Self = spirit;
		_abreviation = abrev;
		_notEmpowered = notEmpowered;
		_empowered = empowered;
	}

	public SpaceState? Space => _spaceCounts.Keys
		.Where( SpiritIsland.Space.Exists )
		.Tokens()
		.SingleOrDefault(); // 0 or 1

	public bool Empowered { get; set; }

	public Img Img => Empowered ? _empowered: _notEmpowered;

	public string Text => SpaceAbreviation;

	public string SpaceAbreviation => _abreviation + (Empowered ? "+" : "-");

	ITokenClass IToken.Class => this;

	#region IEntityClass properties
	public string Label => "My incarna???";

	public bool HasTag(ITag tag) => tag == this || tag == Self.Presence || tag == TokenCategory.Incarna; // Class and entity

	#endregion

	public async Task MoveTo( Space destination, bool allowAdd ) {
		if(Space != null)
			await this.On( Space.Space ).MoveTo( destination );
		else if(allowAdd)
			await destination.Tokens.Add( this, 1 );

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
}
