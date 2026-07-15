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
	public class StopRavageIfTooFewExplorers : IConfigRavages, IEndWhenTimePasses, IRavageSequenceStep {
		Task IConfigRavages.Config(Space space) {
			var steps = space.RavageBehavior.SequenceSteps;
			if( !steps.Contains(this) )
				steps.Add(this);
			return Task.CompletedTask;
		}

		public Task Execute(RavageBehavior behavior, RavageData data, Func<Task> next) {
			int ravagingExplorers = data.Space.HumanOfTag(Human.Explorer)
				.Where(token => token.RavageSide == Invaders.Ravage.RavageSide.Attacker)
				.Select(token => data.Space[token])
				.Sum();
			return (3 <= ravagingExplorers)
				? next()
				: Task.CompletedTask;
		}

	}

}
