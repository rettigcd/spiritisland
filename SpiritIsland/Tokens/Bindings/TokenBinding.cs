namespace SpiritIsland;

public class TokenBindingNoEvents {
	readonly protected SpaceState _tokens;
	readonly protected IVisibleToken _token;

	#region constructor
	public TokenBindingNoEvents( SpaceState tokens, IVisibleToken token ) {
		_tokens = tokens;
		_token = token;
	}
	public TokenBindingNoEvents( TokenBindingNoEvents src ) {
		_tokens = src._tokens;
		_token = src._token;
	}
	#endregion

	public bool Any => Count > 0;

	public virtual int Count => _tokens[_token];

	public void Init( int count ) => _tokens.Init( _token, count );

	public void Adjust( int delta ) => _tokens.Adjust( _token, delta );

	public TokenBinding BindScope() => new TokenBinding(this);
}

public class TokenBinding : TokenBindingNoEvents {

	#region constructor

	public TokenBinding( TokenBindingNoEvents src ) : base( src ) {
		_actionTokens = _tokens.BindScope();
	}

	#endregion

	public virtual Task Add( int count, AddReason reason = AddReason.Added )
		=> _actionTokens.Add( _token, count, reason );

	public virtual Task Remove( int count, RemoveReason reason = RemoveReason.Removed ) 
		=> _actionTokens.Remove( _token, count, reason );

	public Task Destroy( int count ) => Remove( count, RemoveReason.Destroyed );

	public static implicit operator int( TokenBinding b ) => b.Count;

	readonly ActionableSpaceState _actionTokens;

}








/// <summary>
/// Binds to Unique tokens but uses the CLASS to search for ancillary Tokens like ManyMinds-Beast Token
/// </summary>
public class BeastBinding_NoEvents {
	readonly protected SpaceState _spaceState;
	readonly protected UniqueToken _uniqueToken;

	#region constructor
	public BeastBinding_NoEvents( SpaceState spaceState, UniqueToken defaultToken ) {
		_spaceState = spaceState;
		_uniqueToken = defaultToken;
	}
	public BeastBinding_NoEvents( BeastBinding_NoEvents src ) {
		_spaceState = src._spaceState;
		_uniqueToken = src._uniqueToken;
	}
	#endregion

	public bool Any => Count > 0;

	public virtual int Count => _spaceState.Sum( _uniqueToken );

	public void Init( int count ) => _spaceState.Init( _uniqueToken, count );

	public void Adjust( int delta ) => _spaceState.Adjust( _uniqueToken, delta );

	public BeastBinding BindScope() => new BeastBinding( this );
}

public class BeastBinding : BeastBinding_NoEvents {

	#region constructor

	public BeastBinding( BeastBinding_NoEvents src ) : base( src ) {
		_actionTokens = _spaceState.BindScope();
	}

	#endregion

	public virtual Task Add( int count, AddReason reason = AddReason.Added )
		=> _actionTokens.Add( _uniqueToken, count, reason );

	public virtual Task Remove( int count, RemoveReason reason = RemoveReason.Removed )
		=> _actionTokens.Remove( _uniqueToken, count, reason );

	public Task Destroy( int count ) => Remove( count, RemoveReason.Destroyed );

	public static implicit operator int( BeastBinding b ) => b.Count;

	readonly ActionableSpaceState _actionTokens;

}
