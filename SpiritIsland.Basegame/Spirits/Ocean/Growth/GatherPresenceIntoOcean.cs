namespace SpiritIsland.Basegame;

public class GatherPresenceIntoOcean : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {

		List<SpaceState> gatherSpaces = ctx.Presence.ActiveSpaceStates
			.Where( p => p.Space.IsCoastal )
			.Select( p => p.Adjacent.Single( o => o.Space.IsOcean ) )
			.Distinct()
			.ToList();

		while(0 < gatherSpaces.Count){

			SpaceState currentTarget = gatherSpaces[0];
			Space source = await ctx.Decision( new Select.Space(
				$"Select source of Presence to Gather into {currentTarget.Space}"
				, currentTarget.Adjacent
					.Where( adjState => ctx.Self.Presence.IsOn(adjState) )
				, Present.Always
			));

			// apply...
			await ctx.Presence.Move( source, currentTarget.Space );

			// next
			gatherSpaces.RemoveAt( 0 );

		} // while
	}

}