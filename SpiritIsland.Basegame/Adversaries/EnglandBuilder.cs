namespace SpiritIsland.Basegame;

public class EnglandBuilder : BuildEngine {
	public override bool ShouldBuildOnSpace( Space space ) {
		return base.ShouldBuildOnSpace( space )
			|| IsAdjacentTo2OrMoreCitiesOrTowns( space );
	}
	static bool IsAdjacentTo2OrMoreCitiesOrTowns( Space space ) => 2 <= space.Adjacent_ForInvaders.Sum( adj => CityTownCounts( adj ) );
	static int CityTownCounts( Space space ) => space.SumAny( Human.Town_City );

}