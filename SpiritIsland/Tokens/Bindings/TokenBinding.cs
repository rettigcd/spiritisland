namespace SpiritIsland;

public class TokenBinding {

	public IVisibleToken Default { get; }

	#region constructor
	public TokenBinding( SpaceState tokens, IVisibleToken token ) {
		_tokens = tokens;
		Default = token;
	}

	public TokenBinding( TokenBinding src ) {
		_tokens = src._tokens;
		Default = src.Default;
	}
	#endregion

	public bool Any => Count > 0;

	public virtual int Count => _tokens[Default];

	public void Init( int count ) => _tokens.Init( Default, count );

	public void Adjust( int delta ) => _tokens.Adjust( Default, delta );

	public virtual Task Add( int count, AddReason reason = AddReason.Added )
		=> _tokens.Add( Default, count, reason );

	public virtual Task Remove( int count, RemoveReason reason = RemoveReason.Removed )
		=> _tokens.Remove( Default, count, reason );

	public static implicit operator int( TokenBinding b ) => b.Count;

	readonly protected SpaceState _tokens;
}




/// <summary>
/// Binds to Unique tokens but uses the CLASS to search for ancillary Tokens like ManyMinds-Beast Token
/// </summary>
public class BeastBinding {
	readonly protected SpaceState _spaceState;
	readonly protected UniqueToken _uniqueToken;

	#region constructor
	public BeastBinding( SpaceState spaceState, UniqueToken defaultToken ) {
		_spaceState = spaceState;
		_uniqueToken = defaultToken;
	}
	public BeastBinding( BeastBinding src ) {
		_spaceState = src._spaceState;
		_uniqueToken = src._uniqueToken;
	}
	#endregion

	public bool Any => Count > 0;

	public virtual int Count => _spaceState.Sum( _uniqueToken );

	public void Init( int count ) => _spaceState.Init( _uniqueToken, count );

	public void Adjust( int delta ) => _spaceState.Adjust( _uniqueToken, delta );

	public Task Add( int count, AddReason reason = AddReason.Added )
		=> _spaceState.Add( _uniqueToken, count, reason );

	public Task Remove( int count, RemoveReason reason = RemoveReason.Removed )
		=> _spaceState.Remove( _uniqueToken, count, reason );

	public Task Destroy( int count ) => Remove( count, RemoveReason.Destroyed );

}
