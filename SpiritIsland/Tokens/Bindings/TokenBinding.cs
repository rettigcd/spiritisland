namespace SpiritIsland;

public class TokenBinding {

	public IToken Default { get; }

	#region constructor
	public TokenBinding( Space space, IToken token ) {
		_space = space;
		Default = token;
	}

	public TokenBinding( TokenBinding src ) {
		_space = src._space;
		Default = src.Default;
	}
	#endregion

	public bool Any => Count > 0;

	public virtual int Count => _space[Default];

	public void Init( int count ) => _space.Init( Default, count );

	public void Adjust( int delta ) => _space.Adjust( Default, delta );

	public virtual Task AddAsync( int count, AddReason reason = AddReason.Added )
		=> _space.AddAsync( Default, count, reason );

	public virtual Task<ITokenRemovedArgs> Remove( int count, RemoveReason reason = RemoveReason.Removed )
		=> _space.RemoveAsync( Default, count, reason );

	public static implicit operator int( TokenBinding b ) => b.Count;

	readonly protected Space _space;
}




/// <summary>
/// Binds to Unique tokens but uses the CLASS to search for ancillary Tokens like ManyMinds-Beast Token
/// </summary>
public class BeastBinding {
	readonly protected Space _space;
	readonly protected TokenClassToken _uniqueToken;

	#region constructor
	public BeastBinding( Space space, TokenClassToken defaultToken ) {
		_space = space;
		_uniqueToken = defaultToken;
	}
	public BeastBinding( BeastBinding src ) {
		_space = src._space;
		_uniqueToken = src._uniqueToken;
	}
	#endregion

	public bool Any {  
		get { 
			int c= Count;
			return 0 < c;
		}
	}

	public virtual int Count => _space.Sum( _uniqueToken );

	public void Init( int count ) => _space.Init( _uniqueToken, count );

	public void Adjust( int delta ) => _space.Adjust( _uniqueToken, delta );

	public Task AddAsync( int count, AddReason reason = AddReason.Added )
		=> _space.AddAsync( _uniqueToken, count, reason );

	public Task Remove( int count, RemoveReason reason = RemoveReason.Removed )
		=> _space.RemoveAsync( _uniqueToken, count, reason );

	public Task Destroy( int count ) => Remove( count, RemoveReason.Destroyed );

}
