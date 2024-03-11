namespace SpiritIsland.Basegame;

public class GatherPresenceIntoOcean : SpiritAction {

	public GatherPresenceIntoOcean():base( "Gather 1 Presence into EACH Ocean" ) { }
	public override async Task ActAsync( Spirit self ) {

		List<Space> gatherSpaces = self.Presence.Lands
			.Where( p => p.SpaceSpec.IsCoastal )
			.Select( p => p.Adjacent_Existing.Single( o => o.SpaceSpec.IsOcean ) ) // Ocean is not in Play during Growth
			.Distinct()
			.ToList();

		while(0 < gatherSpaces.Count){

			Space currentTarget = gatherSpaces[0];

			var source = await self.SelectAsync( new A.SpaceTokenDecision(
				$"Select source of Presence to Gather into {currentTarget.SpaceSpec}"
				, self.Presence.Deployed.Where( d => self.Presence.IsOn( d.Space ) )
				, Present.Always
			));

			// apply...
			await source.MoveTo( currentTarget );

			// next
			gatherSpaces.RemoveAt( 0 );

		} // while
	}

}