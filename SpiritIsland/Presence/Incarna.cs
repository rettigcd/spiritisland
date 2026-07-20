namespace SpiritIsland;

#nullable enable
public class Incarna( Spirit _spirit, string _abrev, Img _notEmpowered, Img _empowered )
	: IToken
	, ITokenClass
	, ITrackMySpaces
	, ISerializableSpaceEntity
{
	static readonly FakeSpace NullSpace = new FakeSpace( "Incarna-Null-Space" ); // 1 null space for all Incarna

	public Spirit Self { get; } = _spirit;

	public SpaceToken AsSpaceToken() => this.On(Space); // Assumes Space is not null.

	public bool Empowered { get; set; }

	string IToken.Badge => Empowered ? "+" : string.Empty;

	public Img Img => Empowered ? _empowered: _notEmpowered;

	string IOption.Text => SpaceAbreviation;

	public string SpaceAbreviation => _abrev + (Empowered ? "+" : "-");

	public ITokenClass Class => this;

	#region IEntityClass properties

	string ITag.Label => $"{Self.SpiritName} Incarna";

	// Class & Token method
	public bool HasTag(ITag tag) {
		return tag == this 
			|| tag == TokenCategory.Incarna // Is: This Incarna token or any Incarna
			|| tag == Self.Presence
			|| tag == TokenCategory.Presence;
	}

	#endregion

	#region public Location (space) properties

	public Space Space => _spaceCounts.Keys
		.Where( ss => SpaceSpec.Exists(ss.SpaceSpec) )
		.SingleOrDefault() ?? NullSpace.ScopeSpace;

	public bool IsPlaced => _spaceCounts.Count != 0;

	// !!! check that SpiritActions that Move/Add Invarna use this.
	public async Task MoveTo(Space destination, bool allowAdd) {
		if( IsPlaced )
			await this.On(Space).MoveTo(destination);
		else if( allowAdd )
			await destination.AddAsync(this, 1);
	}

	#endregion

	#region ITrackMySpaces imp

	void ITrackMySpaces.Clear() {
		_spaceCounts.Clear();
	}

	void ITrackMySpaces.TrackAdjust(Space space, int delta) {
		_spaceCounts[space] += delta;
		if (1 < _spaceCounts.Count)
			throw new Exception("Incarna is in 2 places at the same time!");
	}

	#endregion ITrackMySpaces imp

	readonly CountDictionary<Space> _spaceCounts = [];

	// _spaceCounts isn't captured - same ITrackMySpaces cache exception as SpiritPresenceToken:
	// it's rebuilt from normal Add/Move token-tracking as the game replays forward.
	//
	// Resolved via spirit index rather than reconstructed, so this always returns the *same*
	// Incarna instance a Spirit already owns (spirit.Presence.Incarna), not a fresh one - this
	// also matters because Incarna implements ITokenClass, and SerializeToken checks
	// ISerializableSpaceEntity before ITokenClass, so this takes priority over (and fixes) the
	// generic by-label fallback, which would have failed: TokenClassRegistry only scans Token/Human
	// static holders, so a per-spirit Incarna was never resolvable that way.
	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, ctx.IndexOf( Self ), Empowered );

	const string Tag = "Incarna";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, FromJson );

	static object FromJson( JsonArray json, ISerializationContext ctx ) {
		Incarna incarna = ctx.SpiritAt( (int)json[1]! ).Presence.Incarna;
		incarna.Empowered = json[2]!.GetValue<bool>();
		return incarna;
	}
}
