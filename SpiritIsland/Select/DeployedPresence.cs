
namespace SpiritIsland.Select;

public static class DeployedPresence {

	public static Space ToPush( ReadOnlyBoundPresence presence, Present present=Present.Done ) 
		=> All("Select Presence to push.", presence, present);

	static public Space ToDestroy(string prompt, ReadOnlyBoundPresence presence )
		=> All(prompt, presence, Present.Always);

	static public Space ToDestroy(string prompt, ReadOnlyBoundPresence presence, Func<SpiritIsland.SpaceState,bool> filter)
		=> Some(prompt, presence, filter, Present.Always);

	static public Space ToDestroy(string prompt, IVisibleToken presenceToken, IEnumerable<SpiritIsland.SpaceState> spaces ) 
		=> new Space( prompt, spaces, Present.Always, presenceToken );

	/// <summary> Targets ALL spaces containing deployed presence </summary>
	/// !!! figure out different reasons .All is called and pull some of the generic ones into this class as factory methods
	static public Space All(string prompt, ReadOnlyBoundPresence presence, Present present )
		=> new Space( prompt, presence.SpaceStates, present, presence.Token );

	/// <summary> Targets ALL spaces containing deployed presence </summary>
	/// !!! figure out different reasons .All is called and pull some of the generic ones into this class as factory methods
	static public Space Movable( string prompt, ReadOnlyBoundPresence presence, Present present )
		=> new Space( prompt, presence.MovableSpaceStates, present, presence.Token );

	static public Space Some(string prompt, ReadOnlyBoundPresence presence, Func<SpiritIsland.SpaceState,bool> filter, Present present )
		=> new Space( prompt, presence.SpaceStates.Where(filter), present, presence.Token );

	/// <summary> Targets Sacred Sites </summary>
	static public Space SacredSites(string prompt, ReadOnlyBoundPresence presence, Present present )
		=> new Space( prompt, presence.SacredSites, present, presence.Token );

	static public TokenFromManySpaces Gather(string prompt, SpiritIsland.Space to, IEnumerable<SpiritIsland.SpaceState> from, Token presenceToken ) 
		=> TokenFromManySpaces.ToCollect( prompt, from.Select(x=>new SpaceToken(x.Space,(IVisibleToken)presenceToken)), Present.Done, to );

}