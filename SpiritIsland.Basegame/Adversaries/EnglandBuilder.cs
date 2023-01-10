namespace SpiritIsland.Basegame;

public class EnglandBuilder : BuildEngine {
	public override bool ShouldBuildOnSpace( SpaceState spaceState ) {
		return base.ShouldBuildOnSpace( spaceState )
			|| IsAdjacentTo2OrMoreCitiesOrTowns( spaceState );
	}
	static bool IsAdjacentTo2OrMoreCitiesOrTowns( SpaceState tokens ) => !tokens.Has( TokenType.Isolate )
		&& 2 <= tokens.Adjacent.Sum( adj => CityTownCounts( adj ) );
	static int CityTownCounts( SpaceState space ) => space.SumAny( Invader.Town_City );

}