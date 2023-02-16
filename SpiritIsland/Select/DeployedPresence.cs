
namespace SpiritIsland.Select;

public static class DeployedPresence {

	public static ASpace ToPush( SpiritPresence presence, Present present=Present.Done ) 
		=> All("Select Presence to push.", presence, present);

	static public ASpace ToDestroy(string prompt, SpiritPresence presence )
		=> All(prompt, presence, Present.Always);

	static public ASpace ToDestroy(string prompt, SpiritPresence presence, Func<SpaceState,bool> filter)
		=> Some(prompt, presence, filter, Present.Always);

	static public ASpace ToDestroy(string prompt, IToken presenceToken, IEnumerable<SpaceState> spaces ) 
		=> new ASpace( prompt, spaces, Present.Always, presenceToken );

	/// <summary> Targets ALL spaces containing deployed presence </summary>
	static public ASpace All(string prompt, SpiritPresence presence, Present present )
		=> new ASpace( prompt, presence.Spaces.Tokens(), present, presence.Token );

	/// <summary> Targets ALL spaces containing deployed presence </summary>
	static public ASpace Movable( string prompt, Spirit spirit, Present present )
		=> new ASpace( prompt, spirit.Presence.CanMove ? spirit.Presence.Spaces.Tokens() : Enumerable.Empty<SpaceState>(), present, spirit.Token );

	static public ASpace Some(string prompt, SpiritPresence presence, Func<SpaceState,bool> filter, Present present )
		=> new ASpace( prompt, presence.Spaces.Tokens().Where(filter), present, presence.Token );

	/// <summary> Targets Sacred Sites </summary>
	static public ASpace SacredSites(string prompt, SpiritPresence presence, Present present )
		=> new ASpace( prompt, presence.SacredSites, present, presence.Token );

	static public TokenFromManySpaces Gather(string prompt, Space to, IEnumerable<SpiritIsland.SpaceState> from, ISpaceEntity presenceToken ) 
		=> TokenFromManySpaces.ToCollect( prompt, from.Select(x=>new SpaceToken(x.Space,(IToken)presenceToken)), Present.Done, to );

}