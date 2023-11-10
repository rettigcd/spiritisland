namespace SpiritIsland.A;
using Orig_Space = SpiritIsland.Space;

/// <summary> Decision that selects a space. </summary>
/// <remarks> Renamed from Select.Space to avoid class name confusion. </remarks>
public class Space : TypedDecision<Orig_Space>, IHaveArrows {

	#region Moving (existing) SpaceTokens

	static public Space ToPushPresence( Orig_Space source, IEnumerable<SpaceState> destinationOptions, Present present, IToken presenceToken )
		=> Space.ForMoving_SpaceToken("Push Presence to", source, destinationOptions, present, presenceToken );

	static public Space ToMoveToken( Orig_Space source, IEnumerable<SpaceState> destinationOptions, Present present, IToken token, int? count = null )
		=> Space.ForMoving_SpaceToken( $"Move {TokensStr(count)} to", source, destinationOptions, present, token );
	static string TokensStr(int? count) => !count.HasValue ? "token(s)" : count.Value == 1 ? "token" : "tokens";

	static public Space ToPushToken( IToken token, Orig_Space source, IEnumerable<SpaceState> destinationOptions, Present present )
		=> Space.ForMoving_SpaceToken( "Push " + token.Text + " to", source, destinationOptions, present, token );

	static public Space ForMoving_SpaceToken( string prompt, Orig_Space source, IEnumerable<SpaceState> spaces, Present present, IToken tokenToAddToTarget ) {
		return new Space( prompt, spaces, present, tokenToAddToTarget ) { _source = source };
	}

	#endregion Moving (existing) SpaceTokens

	#region Placing (new) Token

	static public Space ToPlacePresence( IEnumerable<SpaceState> options, Present present, IToken tokenToAdd )
		=> new Space( "Where would you like to place your presence?", options, present, tokenToAdd );

	static public Space ToPlaceToken( string prompt, IEnumerable<SpaceState> options, Present present, IToken tokenToAdd )
		=> new Space( prompt, options, present, tokenToAdd );

	#endregion Placing (new )Tokens

	#region constructors

	public Space( string prompt, IEnumerable<Orig_Space> spaces, Present present )
		: base( prompt, spaces.OrderBy( x => x.Label ), present ) 
	{
		Spaces = spaces.OrderBy( x => x.Label ).ToArray();
	}

	public Space( string prompt, IEnumerable<SpaceState> spaces, Present present )
		: base( prompt, spaces.Downgrade().OrderBy( x => x.Label ), present ) 
	{
		Spaces = spaces.Downgrade().OrderBy( x => x.Label ).ToArray();
	}

	/// <summary>
	/// Selects a space that will receive a token
	/// </summary>
	public Space( string prompt, IEnumerable<SpaceState> spaces, Present present, IToken tokenToReceive )
		: base( prompt, spaces.Downgrade().OrderBy( x => x.Label ), present ) 
	{
		Token = tokenToReceive;
		Spaces = spaces.Downgrade().OrderBy( x => x.Label ).ToArray();
	}

	#endregion

	public Orig_Space[] Spaces {  get; }

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
	Orig_Space _source;

}
