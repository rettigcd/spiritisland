namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds ravage spaces where 3 or more explorers
/// Level-6 => CheckForPressureForFastProfit - Adds Explorer/Town to each board where no blight was added
/// </summary>
class Russia_Level3_CompetitionAmongHuntersRavageEngine : RavageEngine {

	const string CompetitionName = "Competition Among Hunters";
	protected override bool MatchesCardForRavage( InvaderCard card, Space space ) {
		if(base.MatchesCardForRavage( card, space )) return true;

		bool hasCompetition = 3 <= space.Sum( Human.Explorer );
		if( hasCompetition ) {
			ActionScope.Current.LogDebug( $"{CompetitionName} causes ravage on {space.Label}" );
			space.Init(new StopRavageIfTooFewExplorers(),1);
		}
		return hasCompetition;
	}

	// 1 per space, don't reuse on multiple spaces
	// If Lure's Enthrall the Foreign Explorers, stops them from ravaging, don't do the ravage.
	class StopRavageIfTooFewExplorers : IConfigRavages, IEndWhenTimePasses {
		public Task Config(Space space) {
			if( _old is null ) {
				_old = space.RavageBehavior.RavageSequence;
				space.RavageBehavior.RavageSequence = OnlyRavageIfThereAre3OrMoreExplorers;
			}
			return Task.CompletedTask;
		}

		Task OnlyRavageIfThereAre3OrMoreExplorers(RavageBehavior behavior, RavageData data) {
			var billy = data.Space.HumanOfTag(Human.Explorer);
			int ravagingExplorers = data.Space.HumanOfTag(Human.Explorer)
				.Where(token => token.RavageSide == Invaders.Ravage.RavageSide.Attacker)
				.Select(token => data.Space[token])
				.Sum();
			return (3 <= ravagingExplorers)
				? _old(behavior, data)
				: Task.CompletedTask;
		}

		Func<RavageBehavior, RavageData, Task> _old;
	}

}
