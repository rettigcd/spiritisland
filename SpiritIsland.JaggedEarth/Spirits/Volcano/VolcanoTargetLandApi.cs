namespace SpiritIsland.JaggedEarth;

class VolcanoTargetLandApi : DefaultRangeCalculator {

	public override IEnumerable<Space> GetTargetOptionsFromKnownSource( Spirit self, GameState gameState, TargettingFrom powerType, IEnumerable<Space> source, TargetCriteria tc ) {
		List<Space> spaces = base.GetTargetOptionsFromKnownSource( self, gameState, powerType, source, tc )
			.ToList();

		// Add towers
		if(powerType != TargettingFrom.Innate) {
			var towers = self.Presence.Placed.Where(s=>3 <= self.Presence.CountOn(s)).ToArray();
			if(towers.Length>0)
				spaces.AddRange( base.GetTargetOptionsFromKnownSource(self,gameState,powerType,towers,new TargetCriteria(tc.Range+1,tc.Filter)) );
		}

		return spaces.Distinct();
	}

}