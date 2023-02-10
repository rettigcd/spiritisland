namespace SpiritIsland;


public class BlightTokenBinding : TokenBinding {

	public BlightTokenBinding( SpaceState tokens ) : base( tokens, Token.Blight ) { }

	/// <summary> Allows Power Cards to block blight on this space. </summary>
	public bool Blocked {
		get => _tokens[blockBlightToken] > 0;
		set => _tokens.Init( blockBlightToken, value ? 1 : 0 );
	}

	static readonly BlockBlightToken blockBlightToken = new BlockBlightToken(); // !!! Needs tests that removes at end of round.

	class BlockBlightToken : ITokenWithEndOfRoundCleanup {
		public IEntityClass Class => ActionModTokenClass.Singleton;
		public void EndOfRoundCleanup( SpaceState spaceState ) {
			spaceState.Init(this,0);
		}
	}

	const string BlightAddedStr = "BlightCause";
	static void RecordBlightAdded( AddReason reason ) => ActionScope.Current[BlightAddedStr] = reason;
	public static AddReason GetAddReason() => ActionScope.Current.SafeGet( BlightAddedStr, AddReason.None );

	// Add:
	// take from card
	// cascade
	// hook for Stone to Prevent
	// Event?  So it doesn't need to know about other spaces & gamestate?
	public override async Task Add( int count, AddReason reason = AddReason.Added ) {
		if(Blocked) return;

		RecordBlightAdded( reason );
		await _tokens.Add( Token.Blight, count, reason );
	}

}
