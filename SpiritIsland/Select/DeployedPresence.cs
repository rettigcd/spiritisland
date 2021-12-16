
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Select {

	// Selects Deployed Presence on a space
	// When we have multiple spirits, need to add which spirit this is.
	public class DeployedPresence : TypedDecision<SpiritIsland.Space>, IHaveAdjacentInfo {

		public static DeployedPresence ToPush( SpiritIsland.Spirit spirit, Present present=Present.Done ) 
			=> All("Select Presence to push.", spirit, present);

		static public DeployedPresence ToDestroy(string prompt, SpiritIsland.Spirit spirit)
			=> All(prompt, spirit,Present.Always);

		static public DeployedPresence ToDestroy(string prompt, IEnumerable<SpiritIsland.Space> spaces, Present present ) 
			=> new DeployedPresence( prompt, spaces, present );

		/// <summary> Targets ALL spaces containing deployed presence </summary>
		/// !!! figure out different reasons .All is called and pull some of the generic ones into this class as factory methods
		static public DeployedPresence All(string prompt, SpiritIsland.Spirit spirit, Present present )
			=> new DeployedPresence( prompt, spirit.Presence.Spaces, present);

		static public DeployedPresence Gather(string prompt, SpiritIsland.Space to, IEnumerable<SpiritIsland.Space> from ) 
			=> new DeployedPresence(prompt, from, Present.Done ) {
				AdjacentInfo = new AdjacentInfo {
					Original = to,
					Adjacent = from.ToArray(),
					Direction = AdjacentDirection.Incoming
				}
			};

		#region constructor

		/// <summary> Target SPECIFIC spaces containing deployed presence </summary>
		public DeployedPresence( string prompt, IEnumerable<SpiritIsland.Space> onSpaces, Present present )
			:base( prompt, onSpaces, present )
		{}

		#endregion

		public AdjacentInfo AdjacentInfo { get; set; }

	}

}
