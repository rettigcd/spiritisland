namespace SpiritIsland;

public class BlightTokenBinding : TokenBinding {

	public BlightTokenBinding( TokenCountDictionary tokens ):base( tokens, TokenType.Blight ) {}

	// Add:
	// take from card
	// cascade
	// hook for Stone to Prevent
	// Event?  So it doesn't need to know about other spaces & gamestate?
	public override async Task Add( int count, AddReason reason = AddReason.Added ) {
		if(!Blocked)
			await base.Add( count, reason );
	}

	// Remove:
	// override by trickster
	public override Task Remove( int count, RemoveReason reason = RemoveReason.Removed ) { 
		return base.Remove( count, reason );
	}

	/// <summary> Allows Power Cards to block blight on this space. </summary>
	public bool Blocked {
		get => tokens[blockBlightToken] > 0;
		set => tokens.Init(blockBlightToken,value?1:0);
	}

	static readonly UniqueToken blockBlightToken = new UniqueToken( "block-blight", 'X', Img.None, TokenCategory.None );

}
