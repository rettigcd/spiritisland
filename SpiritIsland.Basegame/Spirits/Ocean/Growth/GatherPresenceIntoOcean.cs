namespace SpiritIsland.Basegame;

public class GatherPresenceIntoOcean : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {

		List<SpaceState> gatherSpaces = ctx.Presence.Spaces
			.Select(s=>ctx.GameState.Tokens[s])
			.Where(s=>!s.InStasis)
			.Where( p => p.Space.IsCoastal )
			.Select( p => p.Adjacent.Single( o => o.Space.IsOcean ) )
			.Distinct()
			.ToList();

		while(0 < gatherSpaces.Count){

			SpaceState currentTarget = gatherSpaces[0];
			Space source = await ctx.Decision( new Select.Space(
				$"Select source of Presence to Gather into {currentTarget.Space}"
				, currentTarget.Adjacent
					.Where( s=>ctx.Presence.Spaces.Contains(s.Space) )
				, Present.Always
			));

			// apply...
			await ctx.Presence.Move( source, currentTarget.Space );

			// next
			gatherSpaces.RemoveAt( 0 );

		} // while
	}

}