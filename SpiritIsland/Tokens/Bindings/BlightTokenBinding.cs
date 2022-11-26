namespace SpiritIsland;


public class BlightTokenBindingNoEvents : TokenBindingNoEvents {

	public BlightTokenBindingNoEvents( SpaceState tokens ) : base( tokens, TokenType.Blight ) { }

	/// <summary> Allows Power Cards to block blight on this space. </summary>
	public bool Blocked {
		get => tokens[blockBlightToken] > 0;
		set => tokens.Init( blockBlightToken, value ? 1 : 0 );
	}

	public new BlightTokenBinding Bind(UnitOfWork guid) => new BlightTokenBinding(tokens, guid);

	static readonly UniqueToken blockBlightToken = new UniqueToken( "block-blight", 'X', Img.None, TokenCategory.None );

}



public class BlightTokenBinding : BlightTokenBindingNoEvents {

	readonly UnitOfWork _actionId;

	public BlightTokenBinding( SpaceState tokens, UnitOfWork actionId )
		:base( tokens ) 
	{
		_actionId = actionId;
	}

	// Add:
	// take from card
	// cascade
	// hook for Stone to Prevent
	// Event?  So it doesn't need to know about other spaces & gamestate?
	public async Task Add( int count, AddReason reason = AddReason.Added ) {
		if(!Blocked)
			await this.tokens.Add( TokenType.Blight, count, _actionId, reason );
	}

	// Remove:
	// override by trickster
	public virtual Task Remove( int count, RemoveReason reason = RemoveReason.Removed ) {
		return this.tokens.Remove( TokenType.Blight, count, _actionId, reason );
	}

}
