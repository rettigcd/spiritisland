using SpiritIsland.Select;

namespace SpiritIsland; 

static public class SpiritSelectExtensions {

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	/// <returns>Track or SpaceToken</returns>
	static public async Task<IOption> SelectSourcePresence( this Spirit self, string actionPhrase = "place" ) {
		string prompt = $"Select Presence to {actionPhrase}";
		return (IOption)await self.Gateway.Decision( TrackSlot.ToReveal( prompt, self ) )
			?? await self.Gateway.Decision( new ASpaceToken( prompt, self.Presence.Deployed, Present.Always ) );
	}

	static public async Task<Space> SelectDeployed( this Spirit self, string prompt )
		=> (await self.Gateway.Decision( new Select.ASpaceToken( prompt, self.Presence.Deployed, Present.Always ) )).Space;

	static public Task<SpaceToken> SelectDeployedMovable( this Spirit self, string prompt )
		=> self.Gateway.Decision( new ASpaceToken( prompt, self.Presence.Movable, Present.Always ) );

}
