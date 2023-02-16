namespace SpiritIsland.Select;

public class ASpace : TypedDecision<Space>, IHaveArrows {

	#region Moving (existing) SpaceTokens

	static public ASpace PushPresence( Space source, IEnumerable<SpaceState> destinationOptions, Present present, IToken presenceToken )
		=> ASpace.ForMoving_SpaceToken("Push Presence to", source, destinationOptions, present, presenceToken );

	static public ASpace MoveToken( Space source, IEnumerable<SpaceState> destinationOptions, Present present, IToken token, int? count = null )
		=> ASpace.ForMoving_SpaceToken( $"Move {TokensStr(count)} to", source, destinationOptions, present, token );
	static string TokensStr(int? count) => !count.HasValue ? "token(s)" : count.Value == 1 ? "token" : "tokens";

	static public ASpace PushToken( IToken token, Space source, IEnumerable<SpaceState> destinationOptions, Present present )
		=> ASpace.ForMoving_SpaceToken( "Push " + token.ToString() + " to", source, destinationOptions, present, token );

	static public ASpace ForMoving_SpaceToken( string prompt, Space source, IEnumerable<SpaceState> spaces, Present present, IToken tokenToAddToTarget ) {
		return new ASpace( prompt, spaces, present, tokenToAddToTarget ) { _source = source };
	}

	#endregion Moving (existing) SpaceTokens

	#region Placing (new) Token

	static public ASpace ToPlacePresence( IEnumerable<SpaceState> options, Present present, IToken tokenToAdd )
		=> new ASpace( "Where would you like to place your presence?", options, present, tokenToAdd );

	static public ASpace ToPlaceToken( string prompt, IEnumerable<SpaceState> options, Present present, IToken tokenToAdd )
		=> new ASpace( prompt, options, present, tokenToAdd );

	#endregion Placing (new )Tokens

	#region constructors

	public ASpace( string prompt, IEnumerable<Space> spaces, Present present )
		: base( prompt, spaces.OrderBy( x => x.Label ), present ) 
	{
		Spaces = spaces.OrderBy( x => x.Label ).ToArray();
	}

	public ASpace( string prompt, IEnumerable<SpaceState> spaces, Present present )
		: base( prompt, spaces.Downgrade().OrderBy( x => x.Label ), present ) 
	{
		Spaces = spaces.Downgrade().OrderBy( x => x.Label ).ToArray();
	}

	/// <summary>
	/// Selects a space that will receive a token
	/// </summary>
	public ASpace( string prompt, IEnumerable<SpaceState> spaces, Present present, IToken tokenToReceive )
		: base( prompt, spaces.Downgrade().OrderBy( x => x.Label ), present ) 
	{
		Token = tokenToReceive;
		Spaces = spaces.Downgrade().OrderBy( x => x.Label ).ToArray();
	}

	#endregion

	public Space[] Spaces {  get; }

	/// <summary> Token to be added to selected space </summary>
	/// <remarks> 
	///	Used for:
	///		pushing/moving/placing: presence, 
	///		pushing/moving: tokens 
	///	</remarks>
	public IToken Token { get; }

	public IEnumerable<Arrow> Arrows => _source == null || Token == null 
		? Enumerable.Empty<Arrow>()
		: Spaces.Select(dstSpace => new Arrow {	Token = Token, From = _source, To = dstSpace } );

	// Only Set when we want to draw outgoing arrows
	Space _source;

}
