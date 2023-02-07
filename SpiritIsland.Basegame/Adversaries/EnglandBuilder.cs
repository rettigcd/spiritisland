namespace SpiritIsland.Basegame;

public class EnglandBuilder : BuildEngine {
	public override bool ShouldBuildOnSpace( SpaceState spaceState ) {
		return base.ShouldBuildOnSpace( spaceState )
			|| IsAdjacentTo2OrMoreCitiesOrTowns( spaceState );
	}
	static bool IsAdjacentTo2OrMoreCitiesOrTowns( SpaceState tokens ) => 2 <= tokens.Adjacent_ForInvaders.Sum( adj => CityTownCounts( adj ) );
	static int CityTownCounts( SpaceState space ) => space.SumAny( Human.Town_City );

}