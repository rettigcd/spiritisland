namespace SpiritIsland.Basegame;

public class GatherPresenceIntoOcean : SpiritAction {

	public GatherPresenceIntoOcean():base( "Gather 1 Presence into EACH Ocean" ) { }
	public override async Task ActAsync( SelfCtx ctx ) {

		List<SpaceState> gatherSpaces = ctx.Self.Presence.Spaces
			.Where( p => p.IsCoastal )
			.Tokens()
			.Select( p => p.Adjacent_Existing.Single( o => o.Space.IsOcean ) ) // Ocean is not in Play during Growth
			.Distinct()
			.ToList();

		while(0 < gatherSpaces.Count){

			SpaceState currentTarget = gatherSpaces[0];

			var source = await ctx.Decision( new A.SpaceToken(
				$"Select source of Presence to Gather into {currentTarget.Space}"
				, ctx.Self.Presence.Deployed.Where( d => ctx.Self.Presence.IsOn( d.Space ) )
				, Present.Always
			));

			// apply...
			await source.MoveTo( currentTarget );

			// next
			gatherSpaces.RemoveAt( 0 );

		} // while
	}

}