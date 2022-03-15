namespace SpiritIsland;

public class BlightTokenBinding : TokenBinding {

	public BlightTokenBinding( TokenCountDictionary tokens ):base( tokens, TokenType.Blight ) {
	}

	// Add:
	// take from card
	// cascade
	// hook for Stone to Prevent
	// Event?  So it doesn't need to know about other spaces & gamestate?
	public override Task Add( int count, AddReason reason = AddReason.Added ) => base.Add( count, reason );

	// Remove:
	// override by trickster
	public override Task Remove( int count, RemoveReason reason = RemoveReason.Removed ) { 
		return base.Remove( count, reason );
	}

}
