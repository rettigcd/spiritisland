namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds ravage spaces where 3 or more explorers
/// Level-6 => CheckForPressureForFastProfit - Adds Explorer/Town to each board where no blight was added
/// </summary>
class Russia_Level3_CompetitionAmongHuntersRavageEngine : RavageEngine {

	const string CompetitionName = "Competition Among Hunters";
	protected override bool MatchesCardForRavage( InvaderCard card, SpaceState spaceState ) {
		if(base.MatchesCardForRavage( card, spaceState )) return true;

		bool hasCompetition = 3 <= spaceState.Sum( Human.Explorer );
		if(hasCompetition)
			GameState.Current.LogDebug( $"{CompetitionName} causes ravage on {spaceState.Space.Text}" );
		return hasCompetition;
	}

}
