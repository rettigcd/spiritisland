namespace SpiritIsland.Basegame;

class SwedenExplorer : ExploreEngine {

	protected override async Task ExploreSingleSpace( SpaceState tokens, GameState gs, bool escalation ) {
		await base.ExploreSingleSpace( tokens, gs, escalation );
		if( escalation )
			SwayedByTheInvaders( tokens, gs );
	}

	static void SwayedByTheInvaders( SpaceState tokens, GameState gs ) {
		var dahan = tokens.Dahan;
		if(0 < dahan.CountAll && dahan.CountAll <= tokens.InvaderTotal()) {
			var dahanToConvert = dahan.NormalKeys.OrderBy( x => x.RemainingHealth ).First();
			var townToAdd = tokens.GetDefaultHuman( Human.Town ).AddDamage( dahanToConvert.Damage );

			dahan.Adjust( dahanToConvert, -1 );
			tokens.Adjust( townToAdd, 1 );
			gs.Log( new Log.InvaderActionEntry( $"Escalation: {tokens.Space.Text} replace {dahanToConvert} with {townToAdd}" ) );
		}
	}

}