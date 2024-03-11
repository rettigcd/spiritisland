namespace SpiritIsland.Basegame;

/// <summary>
/// Overrides the Explore Engine because the escalation action happens during the explore instead of after it
/// </summary>
class SwedenExplorer : ExploreEngine {

	// After Invaders Explore into each land this Phase,
	// if that land has at least as many Invaders as Dahan, replace 1 Dahan with 1 Town.

	// !! Note: if .DoExplore(...) returned the results, we could pass the explore results into the standard Escalation method
	// and do away with this whole class.

	protected override async Task Explore_1Space_Stoppable( Space space, GameState gs, bool escalation ) {
		await base.Explore_1Space_Stoppable( space, gs, escalation );
		if( escalation )
			await SwayedByTheInvadersAsync( space );
	}

	static async Task SwayedByTheInvadersAsync( Space space ) {
		var dahan = space.Dahan;
		if(0 < dahan.CountAll && dahan.CountAll <= space.InvaderTotal()) {
			HumanToken dahanToConvert = dahan.NormalKeys.OrderBy( x => x.RemainingHealth ).First();
			ActionScope.Current.Log( new Log.InvaderActionEntry( $"Escalation: {space.Label} replace {dahanToConvert} with Town" ) );
			await space.ReplaceHumanAsync( dahanToConvert, Human.Town );
		}
	}

}