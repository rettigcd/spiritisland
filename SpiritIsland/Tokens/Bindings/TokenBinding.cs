namespace SpiritIsland;

public class TokenBindingNoEvents {
	readonly protected TokenCountDictionary tokens;
	readonly protected Token token;

	#region constructor
	public TokenBindingNoEvents( TokenCountDictionary tokens, Token token ) {
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

	public TokenBinding Bind(Guid actionId) => new TokenBinding(this,actionId);
}

public class TokenBinding : TokenBindingNoEvents {

	#region constructor

	public TokenBinding( TokenCountDictionary tokens, Token token, Guid actionId ) : base( tokens, token ) {
		if( actionId == default ) throw new ArgumentOutOfRangeException(nameof(actionId),"Action ID cannot be default.");
		this._actionId = actionId;
	}

	public TokenBinding( TokenBindingNoEvents src, Guid actionId ) : base( src ) {
		if(actionId == default) throw new ArgumentOutOfRangeException( nameof( actionId ), "Action ID cannot be default." );
		this._actionId = actionId;
	}

	#endregion

	public virtual Task Add( int count, AddReason reason = AddReason.Added )
		=> tokens.Add( token, count, _actionId, reason );

	public virtual Task Remove( int count, RemoveReason reason = RemoveReason.Removed ) => tokens.Remove( token, count, _actionId, reason );

	public Task Destroy( int count ) => Remove( count, RemoveReason.Destroyed );

	public static implicit operator int( TokenBinding b ) => b.Count;

	readonly Guid _actionId;

}

