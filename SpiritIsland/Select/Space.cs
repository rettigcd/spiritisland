namespace SpiritIsland.Select;

using Type = SpiritIsland.Space;

public class Space : TypedDecision<Type>, IHaveAdjacentInfo {

	#region Adjacent

	static public Space PushPresence(Type source, IEnumerable<Type> destinationOptions, Present present )
		=> Space.ForAdjacent("Push Presence to", source, AdjacentDirection.Outgoing, destinationOptions, present );

	static public Space PushToken( Token token, Type source, IEnumerable<Type> destinationOptions, Present present )
		=> Space.ForAdjacent( "Push " + token.ToString() + " to", source, AdjacentDirection.Outgoing, destinationOptions, present );

	static public Space PushToken( TokenClass tokenClass, Type source, IEnumerable<Type> destinationOptions, Present present )
		=> Space.ForAdjacent( "Push " + tokenClass.Label + " to", source, AdjacentDirection.Outgoing, destinationOptions, present );

	static public Space ForAdjacent( string prompt, Type source, AdjacentDirection gatherPush, IEnumerable<Type> spaces, Present present ) {
		return new Space( prompt, spaces.OrderBy( x => x.Label ), present ) {
			AdjacentInfo = new AdjacentInfo {
				Original = source,
				Direction = gatherPush,
				Adjacent = spaces.ToArray()
			}
		};
	}

	public AdjacentInfo AdjacentInfo { get; set; }

	#endregion

	public static Space ToPlacePresence( IEnumerable<SpiritIsland.Space> destinationOptions, Present present )
		=> new Space( "Where would you like to place your presence?", destinationOptions, present );

	public Space( string prompt, IEnumerable<Type> spaces, Present present )
		: base( prompt, spaces.OrderBy( x => x.Label ), present ) {
	}

}