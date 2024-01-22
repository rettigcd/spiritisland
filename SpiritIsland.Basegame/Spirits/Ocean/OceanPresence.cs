namespace SpiritIsland.Basegame;

public class OceanPresence( Spirit spirit, PresenceTrack energy, PresenceTrack cardPlays ) 
	: SpiritPresence( spirit, energy, cardPlays ) 
{
	public override bool CanBePlacedOn( SpaceState s ) {
		var tm = ActionScope.Current.TerrainMapper;
		return tm.MatchesTerrain( s, Terrain.Ocean ) || tm.IsCoastal( s );
	}

}