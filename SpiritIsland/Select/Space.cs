namespace SpiritIsland.Select;

using SiSpace = SpiritIsland.Space;

public class Space : TypedDecision<SiSpace>, IHaveAdjacentInfo, IHaveTokenInfo {

	#region Adjacent

	static public Space PushPresence(SiSpace source, IEnumerable<SpaceState> destinationOptions, Present present, Token presenceToken )
		=> Space.ForAdjacent("Push Presence to", source, AdjacentDirection.Outgoing, destinationOptions, present, presenceToken );

	public static Space ToPlacePresence( IEnumerable<SpiritIsland.Space> destinationOptions, Present present, Token presenceToken )
		=> new Space( "Where would you like to place your presence?", destinationOptions, present ) { Token = presenceToken };

	static public Space MoveToken( SiSpace source, IEnumerable<SpaceState> destinationOptions, Present present, Token token )
		=> Space.ForAdjacent( "Move token(s) to", source, AdjacentDirection.Outgoing, destinationOptions, present, token );

	static public Space PushToken( Token token, SiSpace source, IEnumerable<SpaceState> destinationOptions, Present present )
		=> Space.ForAdjacent( "Push " + token.ToString() + " to", source, AdjacentDirection.Outgoing, destinationOptions, present, token );

	static public Space ForAdjacent( string prompt, SiSpace source, AdjacentDirection gatherPush, IEnumerable<SpaceState> spaces, Present present, Token tokenToAddToTarget ) {
		return new Space( prompt, spaces.OrderBy( x => x.Space.Label ), present ) {
			AdjacentInfo = new AdjacentInfo {
				Central = source,
				Direction = gatherPush,
				Adjacent = spaces.Select(x=>x.Space).ToArray(),
			},
			Token = tokenToAddToTarget,
		};
	}

	#endregion

	public Space( string prompt, IEnumerable<SiSpace> spaces, Present present )
		: base( prompt, spaces.OrderBy( x => x.Label ), present ) {
	}

	public Space( string prompt, IEnumerable<SpaceState> spaces, Present present )
		: base( prompt, spaces.Select(x=>x.Space).OrderBy( x => x.Label ), present ) {
	}

	/// <summary> Token to be added to selected space </summary>
	/// <remarks> 
	///	Used for:
	///		pushing/moving/placing: presence, 
	///		pushing/moving: tokens 
	///	</remarks>
	public Token Token { get; private set; }
	public AdjacentInfo AdjacentInfo { get; private set; } // Outgoing (pushing/moving)

}