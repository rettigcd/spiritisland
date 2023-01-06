namespace SpiritIsland;

public class TokenBindingNoEvents {
	readonly protected SpaceState tokens;
	readonly protected Token token;

	#region constructor
	public TokenBindingNoEvents( SpaceState tokens, Token token ) {
		this.tokens = tokens;
		this.token = token;
	}
	public TokenBindingNoEvents( TokenBindingNoEvents src ) {
		this.tokens = src.tokens;
		this.token = src.token;
	}
	#endregion

	public bool Any => Count > 0;

	public virtual int Count => tokens[token];

	public void Init( int count ) => tokens.Init( token, count );

	public void Adjust( int delta ) => tokens.Adjust( token, delta );

	public TokenBinding Bind(UnitOfWork actionScope) => new TokenBinding(this,actionScope);
}

public class TokenBinding : TokenBindingNoEvents {

	#region constructor

	public TokenBinding( SpaceState tokens, Token token, UnitOfWork actionScope ) : base( tokens, token ) {
		_actionScope = actionScope ?? throw new ArgumentOutOfRangeException(nameof(actionScope),"Action ID cannot be default.");
	}

	public TokenBinding( TokenBindingNoEvents src, UnitOfWork actionScope ) : base( src ) {
		_actionScope = actionScope ?? throw new ArgumentOutOfRangeException( nameof( actionScope ), "Action ID cannot be default." );
	}

	#endregion

	public virtual Task Add( int count, AddReason reason = AddReason.Added )
		=> tokens.Add( token, count, _actionScope, reason );

	public virtual Task Remove( int count, RemoveReason reason = RemoveReason.Removed ) => tokens.Remove( token, count, _actionScope, reason );

	public Task Destroy( int count ) => Remove( count, RemoveReason.Destroyed );

	public static implicit operator int( TokenBinding b ) => b.Count;

	readonly UnitOfWork _actionScope;

}

