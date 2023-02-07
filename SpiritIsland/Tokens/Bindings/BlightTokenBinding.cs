namespace SpiritIsland;


public class BlightTokenBindingNoEvents : TokenBindingNoEvents {

	public BlightTokenBindingNoEvents( SpaceState tokens ) : base( tokens, Token.Blight ) { }

	/// <summary> Allows Power Cards to block blight on this space. </summary>
	public bool Blocked {
		get => _tokens[blockBlightToken] > 0;
		set => _tokens.Init( blockBlightToken, value ? 1 : 0 );
	}

	public new BlightTokenBinding BindScope() => new BlightTokenBinding(_tokens );

	static readonly BlockBlightToken blockBlightToken = new BlockBlightToken(); // !!! Needs tests that removes at end of round.

	class BlockBlightToken : ITokenWithEndOfRoundCleanup {
		public TokenClass Class => ActionModTokenClass.Singleton;
		public void EndOfRoundCleanup( SpaceState spaceState ) {
			spaceState.Init(this,0);
		}
	}

}



public class BlightTokenBinding : BlightTokenBindingNoEvents {

	readonly ActionableSpaceState _actionTokens;

	public BlightTokenBinding( SpaceState tokens )
		:base( tokens ) 
	{
		_actionTokens = tokens.BindScope();
	}

	// Add:
	// take from card
	// cascade
	// hook for Stone to Prevent
	// Event?  So it doesn't need to know about other spaces & gamestate?
	public async Task Add( int count, AddReason reason = AddReason.Added ) {
		if(Blocked) return;

		RecordBlightAdded( reason );
		await _actionTokens.Add( Token.Blight, count, reason );
	}

	// Remove:
	// override by trickster
	public virtual Task Remove( int count, RemoveReason reason = RemoveReason.Removed ) {
		return _actionTokens.Remove( Token.Blight, count, reason );
	}

	#region Blight Cause

	const string BlightAddedStr = "BlightCause";
	static void RecordBlightAdded( AddReason reason ) => UnitOfWork.Current[BlightAddedStr] = reason;
	public static AddReason GetAddReason() => UnitOfWork.Current.SafeGet( BlightAddedStr, AddReason.None );

	#endregion

}
