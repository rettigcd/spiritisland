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

	public TokenBinding Bind(UnitOfWork actionScope) => new TokenBinding(this,actionScope);
}

public class TokenBinding : TokenBindingNoEvents {

	#region constructor

	public TokenBinding( TokenBindingNoEvents src, UnitOfWork actionScope ) : base( src ) {
		_actionTokens = _tokens.Bind( actionScope ?? throw new ArgumentOutOfRangeException( nameof( actionScope ), "Action ID cannot be default." ) );
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

