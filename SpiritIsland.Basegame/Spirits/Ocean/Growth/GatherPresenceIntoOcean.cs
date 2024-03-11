namespace SpiritIsland.Basegame;

public class GatherPresenceIntoOcean : SpiritAction {

	public GatherPresenceIntoOcean():base( "Gather 1 Presence into EACH Ocean" ) { }
	public override async Task ActAsync( Spirit self ) {

		List<SpaceState> gatherSpaces = self.Presence.Lands
			.Where( p => p.Space.IsCoastal )
			.Select( p => p.Adjacent_Existing.Single( o => o.Space.IsOcean ) ) // Ocean is not in Play during Growth
			.Distinct()
			.ToList();

		while(0 < gatherSpaces.Count){

			SpaceState currentTarget = gatherSpaces[0];

			var source = await self.SelectAsync( new A.SpaceToken(
				$"Select source of Presence to Gather into {currentTarget.Space}"
				, self.Presence.Deployed.Where( d => self.Presence.IsOn( d.Space.ScopeTokens ) )
				, Present.Always
			));

			// apply...
			await source.MoveTo( currentTarget );

			// next
			gatherSpaces.RemoveAt( 0 );

		} // while
	}

}