
namespace SpiritIsland.Select;

public static class DeployedPresence {

	public static Space ToPush( SpiritPresence presence, Present present=Present.Done ) 
		=> All("Select Presence to push.", presence, present);

	static public Space ToDestroy(string prompt, SpiritPresence presence )
		=> All(prompt, presence, Present.Always);

	static public Space ToDestroy(string prompt, SpiritPresence presence, Func<SpiritIsland.SpaceState,bool> filter)
		=> Some(prompt, presence, filter, Present.Always);

	static public Space ToDestroy(string prompt, IToken presenceToken, IEnumerable<SpiritIsland.SpaceState> spaces ) 
		=> new Space( prompt, spaces, Present.Always, presenceToken );

	/// <summary> Targets ALL spaces containing deployed presence </summary>
	static public Space All(string prompt, SpiritPresence presence, Present present )
		=> new Space( prompt, presence.ActiveSpaceStates, present, presence.Token );

	/// <summary> Targets ALL spaces containing deployed presence </summary>
	static public Space Movable( string prompt, SpiritIsland.Spirit spirit, Present present )
		=> new Space( prompt, spirit.Presence.MovableSpaceStates, present, spirit.Token );

	static public Space Some(string prompt, SpiritPresence presence, Func<SpiritIsland.SpaceState,bool> filter, Present present )
		=> new Space( prompt, presence.ActiveSpaceStates.Where(filter), present, presence.Token );

	/// <summary> Targets Sacred Sites </summary>
	static public Space SacredSites(string prompt, SpiritPresence presence, Present present )
		=> new Space( prompt, presence.SacredSiteStates, present, presence.Token );

	static public TokenFromManySpaces Gather(string prompt, SpiritIsland.Space to, IEnumerable<SpiritIsland.SpaceState> from, ISpaceEntity presenceToken ) 
		=> TokenFromManySpaces.ToCollect( prompt, from.Select(x=>new SpaceToken(x.Space,(IToken)presenceToken)), Present.Done, to );

}