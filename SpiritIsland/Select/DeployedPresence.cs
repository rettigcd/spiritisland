namespace SpiritIsland.Select;

// Selects Deployed Presence on a space
// When we have multiple spirits, need to add which spirit this is.
public class DeployedPresence : TypedDecision<SpiritIsland.Space>, IHaveAdjacentInfo {

	public static DeployedPresence ToPush( SpiritIsland.Spirit spirit, GameState gs, Present present=Present.Done ) 
		=> All("Select Presence to push.", spirit, gs, present);

	static public DeployedPresence ToDestroy(string prompt, SpiritIsland.Spirit spirit, GameState gs)
		=> All(prompt, spirit, gs, Present.Always);

	static public DeployedPresence ToDestroy(string prompt, SpiritIsland.Spirit spirit, GameState gs, Func<SpiritIsland.SpaceState,bool> filter)
		=> Some(prompt, spirit, gs, filter, Present.Always);

	static public DeployedPresence ToDestroy(string prompt, IEnumerable<SpiritIsland.SpaceState> spaces, Present present ) 
		=> new DeployedPresence( prompt, spaces, present );

	/// <summary> Targets ALL spaces containing deployed presence </summary>
	/// !!! figure out different reasons .All is called and pull some of the generic ones into this class as factory methods
	static public DeployedPresence All(string prompt, SpiritIsland.Spirit spirit, GameState gs, Present present )
		=> new DeployedPresence( prompt, spirit.BindMyPower(gs).Presence.SpaceStates, present);

	static public DeployedPresence Some(string prompt, SpiritIsland.Spirit spirit, GameState gs, Func<SpiritIsland.SpaceState,bool> filter, Present present )
		=> new DeployedPresence( prompt, spirit.BindMyPower(gs).Presence.SpaceStates.Where(filter), present);

	/// <summary> Targets Sacred Sites </summary>
	static public DeployedPresence SacredSites(string prompt, GameState gs, SpiritIsland.Spirit spirit, TerrainMapper mapper, Present present )
		=> new DeployedPresence( prompt, spirit.BindMyPower(gs).Presence.SacredSites, present);


	static public DeployedPresence Gather(string prompt, SpiritIsland.Space to, IEnumerable<SpiritIsland.SpaceState> from ) 
		=> new DeployedPresence(prompt, from, Present.Done ) {
			AdjacentInfo = new AdjacentInfo {
				Original = to,
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