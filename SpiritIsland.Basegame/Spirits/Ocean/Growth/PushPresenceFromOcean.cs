namespace SpiritIsland.Basegame;

public class PushPresenceFromOcean : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {

		var pushSpaces = ctx.Self.Presence.Spaces( ctx.GameState )
			.Select( x => ctx.GameState.Tokens[x])
			.Where( p => p.Space.IsOcean )
			.Distinct()
			.ToList();

		while(0 < pushSpaces.Count){
			var currentSource = pushSpaces[0];

			// #pushpresence
			var destination = await ctx.Decision( Select.Space.PushPresence( currentSource.Space, currentSource.Adjacent.Select(x=>x.Space), Present.Always ));

			// apply...
			ctx.Presence.Move( currentSource.Space, destination );

			// next
			pushSpaces.RemoveAt( 0 );
		}

	}

}