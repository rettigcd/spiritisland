namespace SpiritIsland.Basegame;

public class GatherPresenceIntoOcean : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		List<Space> gatherSpaces = ctx.Self.Presence.Spaces
			.Where( p => p.IsCoastal )
			.Select( p => p.Adjacent.Single( o => o.IsOcean ) )
			.Distinct()
			.ToList();

		while(0 < gatherSpaces.Count){

			Space currentTarget = gatherSpaces[0];
			Space source = await ctx.Decision( new Select.Space(
				$"Select source of Presence to Gather into {currentTarget}"
				, currentTarget.Adjacent
					.Where( ctx.Self.Presence.Spaces.Contains )
					.ToArray()
				, Present.Always
			));

			// apply...
			ctx.Presence.Move( source, currentTarget );

			// next
			gatherSpaces.RemoveAt( 0 );

		} // while
	}

}