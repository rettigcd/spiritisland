namespace SpiritIsland.A;
using Orig_Space = SpiritIsland.Space;
using IEnumerable_Spaces = IEnumerable<SpiritIsland.Space>;

/// <summary> Decision that selects a space. </summary>
/// <remarks> Renamed from Select.Space to avoid class name confusion. </remarks>
public class Space : TypedDecision<Orig_Space>, IHaveArrows {

	#region Moving (existing) SpaceTokens

	static public Space ToPushPresence( Orig_Space source, IEnumerable_Spaces destinationOptions, Present present, IToken presenceToken )
		=> Space.ForMoving_SpaceToken("Push Presence to", source, destinationOptions, present, presenceToken );

	static public Space ToMoveToken( SpiritIsland.SpaceToken spaceToken, IEnumerable_Spaces destinationOptions, Present present, int? count = null )
		=> Space.ForMoving_SpaceToken( $"Move {TokensStr(count)} to", spaceToken.Space, destinationOptions, present, spaceToken.Token );
	static string TokensStr(int? count) => !count.HasValue ? "token(s)" : count.Value == 1 ? "token" : "tokens";

	static public Space ToPushToken( IToken token, Orig_Space source, IEnumerable_Spaces destinationOptions, Present present )
		=> Space.ForMoving_SpaceToken( "Push " + token.Text + " to", source, destinationOptions, present, token );

	static public Space ForMoving_SpaceToken( string prompt, Orig_Space source, IEnumerable_Spaces spaces, Present present, IToken tokenToAddToTarget )
		=> new Space( prompt, spaces, present )
			.ComingFrom( source )
			.ShowTokenLocation( tokenToAddToTarget );

	#endregion Moving (existing) SpaceTokens

	#region Placing (new) Token

	static public Space ToPlacePresence( IEnumerable_Spaces options, Present present, IToken tokenToAdd )
		=> new Space( "Where would you like to place your presence?", options, present )
			.ShowTokenLocation( tokenToAdd );

	static public Space ToPlacePresence(IEnumerable<SpaceState> options, Present present, IToken tokenToAdd)
		=> new Space("Where would you like to place your presence?", options, present)
			.ShowTokenLocation(tokenToAdd);

	static public Space ToPlaceDestroyedPresence( IEnumerable_Spaces options, Present present, SpiritIsland.Spirit spirit, int? count=null )
		// make sure caller has pre-filtered spaces using:  .Where( spirit.Presence.CanBePlacedOn )
		=> new Space(
				!count.HasValue 
					? $"Place Destroyed Presence" 
				: present == Present.Always 
					? $"Place up to {count.Value} Destroyed Presence"
				: $"Place {count.Value} Destroyed Presence",
				options, 
				present
			).ShowTokenLocation( spirit.Presence.Token );

	static public Space ToPlaceToken( string prompt, IEnumerable_Spaces options, Present present, IToken tokenToAdd )
		=> new Space( prompt, options, present )
			.ShowTokenLocation( tokenToAdd );

	#endregion Placing (new )Tokens

	#region constructors

	/// <remarks> Convenience method - downgrades SpaceStates to Spaces</remarks>
	public Space( string prompt, IEnumerable<SpaceState> spaces, Present present )
		: this( prompt, spaces.Downgrade(), present ) { }

	public Space(string prompt, IEnumerable<SpaceState> spaces, string cancelText)
		: this(prompt, spaces.Downgrade(), cancelText) { }

	public Space( string prompt, IEnumerable<Orig_Space> spaces, Present present )
		: base( prompt, spaces.OrderBy( x => x.Label ), present ) 
	{
		Spaces = [..spaces.OrderBy( x => x.Label )];
	}

	public Space( string prompt, IEnumerable<Orig_Space> spaces, string cancelText )
		: base( prompt, spaces.OrderBy( x => x.Label ), cancelText ) 
	{
		Spaces = [..spaces.OrderBy( x => x.Label )];
	}


	#endregion

	public Orig_Space[] Spaces {  get; }

	#region config: Token circle

	/// <summary>
	/// When moving a token into a space, causes the space the token will be displayed in as the hot-spot.
	/// </summary>
	public Space ShowTokenLocation( IToken tokenToReceive ) {
		Token = tokenToReceive;
		return this;
	}

	/// <summary> Token to be added to selected space </summary>
	/// <remarks> 
	///	Used for:
	///		pushing/moving/placing: presence, 
	///		pushing/moving: tokens 
	///	</remarks>
	public IToken Token { 
		get; 
		private set;
	}

	#endregion config: Token circle

	#region config: Arrows
	public Space ComingFrom( Orig_Space source ) {
		_source = source;
		return this;
	}

	public IEnumerable<Arrow> Arrows => _source == null || Token == null 
		? Enumerable.Empty<Arrow>()
		: Spaces.Select(dstSpace => new Arrow {	Token = Token, From = _source, To = dstSpace } );

	// Only Set when we want to draw outgoing arrows
	Orig_Space _source;

	#endregion config: Arrows


}
