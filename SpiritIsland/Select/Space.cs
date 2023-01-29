namespace SpiritIsland.Select;

public class Space : TypedDecision<SpiritIsland.Space>, IHaveArrows {

	#region Moving (existing) SpaceTokens

	static public Space PushPresence( SpiritIsland.Space source, IEnumerable<SpaceState> destinationOptions, Present present, IVisibleToken presenceToken )
		=> Space.ForMoving_SpaceToken("Push Presence to", source, destinationOptions, present, presenceToken );

	static public Space MoveToken( SpiritIsland.Space source, IEnumerable<SpaceState> destinationOptions, Present present, IVisibleToken token )
		=> Space.ForMoving_SpaceToken( "Move token(s) to", source, destinationOptions, present, token );

	static public Space PushToken( IVisibleToken token, SpiritIsland.Space source, IEnumerable<SpaceState> destinationOptions, Present present )
		=> Space.ForMoving_SpaceToken( "Push " + token.ToString() + " to", source, destinationOptions, present, token );

	static public Space ForMoving_SpaceToken( string prompt, SpiritIsland.Space source, IEnumerable<SpaceState> spaces, Present present, IVisibleToken tokenToAddToTarget ) {
		return new Space( prompt, spaces, present, tokenToAddToTarget ) { Source = source };
	}

	#endregion Moving (existing) SpaceTokens

	#region Placing (new) Token

	static public Space ToPlacePresence( IEnumerable<SpiritIsland.SpaceState> options, Present present, IVisibleToken tokenToAdd )
		=> new Space( "Where would you like to place your presence?", options, present, tokenToAdd );

	static public Space ToPlaceToken( string prompt, IEnumerable<SpaceState> options, Present present, IVisibleToken tokenToAdd )
		=> new Space( prompt, options, present, tokenToAdd );

	#endregion Placing (new )Tokens

	#region constructors

	public Space( string prompt, IEnumerable<SpiritIsland.Space> spaces, Present present )
		: base( prompt, spaces.OrderBy( x => x.Label ), present ) 
	{
		Spaces = spaces.OrderBy( x => x.Label ).ToArray();
	}

	public Space( string prompt, IEnumerable<SpaceState> spaces, Present present )
		: base( prompt, spaces.Select(x=>x.Space).OrderBy( x => x.Label ), present ) 
	{
		Spaces = spaces.Select( x => x.Space ).OrderBy( x => x.Label ).ToArray();
	}

	/// <summary>
	/// Selects a space that will receive a token
	/// </summary>
	public Space( string prompt, IEnumerable<SpaceState> spaces, Present present, IVisibleToken tokenToReceive )
		: base( prompt, spaces.Select( x => x.Space ).OrderBy( x => x.Label ), present ) 
	{
		Token = tokenToReceive;
		Spaces = spaces.Select( x => x.Space ).OrderBy( x => x.Label ).ToArray();
	}

	#endregion

	public SpiritIsland.Space[] Spaces {  get; }

	/// <summary> Token to be added to selected space </summary>
	/// <remarks> 
	///	Used for:
	///		pushing/moving/placing: presence, 
	///		pushing/moving: tokens 
	///	</remarks>
	public IVisibleToken Token { get; }

	// Only Set when we want to draw outgoing arrows
	// !!! return Arrow enumerator, then make this private
	public SpiritIsland.Space Source { get; set; } 

	public IEnumerable<Arrow> Arrows => Source == null || Token == null 
		? Array.Empty<Arrow>()
		: Spaces.Select(dstSpace => new Arrow {	Token = Token, From = Source, To = dstSpace } );

}
