namespace SpiritIsland.Basegame;

public class GatherPresenceIntoOcean : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {

		List<SpaceState> gatherSpaces = ctx.Self.Presence.SpaceStates
			.Where( p => p.Space.IsCoastal )
			.Select( p => p.Adjacent_Unfiltered.Single( o => o.Space.IsOcean ) ) // Ocean is not in Play during Growth
			.Distinct()
			.ToList();

		while(0 < gatherSpaces.Count){

			SpaceState currentTarget = gatherSpaces[0];
			Space source = await ctx.Decision( new Select.ASpace(
				$"Select source of Presence to Gather into {currentTarget.Space}"
				, currentTarget.Adjacent
					.Where( adjState => adjState.Has(ctx.Self.Token) )
				, Present.Always
			));

			// apply...
			await ctx.Self.Token.Move( source.Tokens, currentTarget );

			// next
			gatherSpaces.RemoveAt( 0 );

		} // while
	}

}