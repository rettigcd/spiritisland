namespace SpiritIsland.Select;

// Selects Deployed Presence on a space
// When we have multiple spirits, need to add which spirit this is.
public class DeployedPresence : TypedDecision<SpiritIsland.Space>, IHaveAdjacentInfo {

	public static DeployedPresence ToPush( ReadOnlyBoundPresence presence, Present present=Present.Done ) 
		=> All("Select Presence to push.", presence, present);

	static public DeployedPresence ToDestroy(string prompt, ReadOnlyBoundPresence presence )
		=> All(prompt, presence, Present.Always);

	static public DeployedPresence ToDestroy(string prompt, ReadOnlyBoundPresence presence, Func<SpiritIsland.SpaceState,bool> filter)
		=> Some(prompt, presence, filter, Present.Always);

	static public DeployedPresence ToDestroy(string prompt, IEnumerable<SpiritIsland.SpaceState> spaces, Present present ) 
		=> new DeployedPresence( prompt, spaces, present );

	/// <summary> Targets ALL spaces containing deployed presence </summary>
	/// !!! figure out different reasons .All is called and pull some of the generic ones into this class as factory methods
	static public DeployedPresence All(string prompt, ReadOnlyBoundPresence presence, Present present )
		=> new DeployedPresence( prompt, presence.SpaceStates, present);

	static public DeployedPresence Some(string prompt, ReadOnlyBoundPresence presence, Func<SpiritIsland.SpaceState,bool> filter, Present present )
		=> new DeployedPresence( prompt, presence.SpaceStates.Where(filter), present);

	/// <summary> Targets Sacred Sites </summary>
	static public DeployedPresence SacredSites(string prompt, ReadOnlyBoundPresence presence, Present present )
		=> new DeployedPresence( prompt, presence.SacredSites, present);


	static public DeployedPresence Gather(string prompt, SpiritIsland.Space to, IEnumerable<SpiritIsland.SpaceState> from ) 
		=> new DeployedPresence(prompt, from, Present.Done ) {
			AdjacentInfo = new AdjacentInfo {
				Central = to,
				Adjacent = from.Select(x=>x.Space).ToArray(),
				Direction = AdjacentDirection.Incoming
			}
		};

	#region constructor

	/// <summary> Target SPECIFIC spaces containing deployed presence </summary>
	public DeployedPresence( string prompt, IEnumerable<SpiritIsland.SpaceState> onSpaces, Present present )
		:base( prompt, onSpaces.Select(x=>x.Space), present )
	{}

	#endregion

	public AdjacentInfo AdjacentInfo { get; set; }

}