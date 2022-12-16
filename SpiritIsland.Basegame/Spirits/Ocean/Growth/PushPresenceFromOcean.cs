namespace SpiritIsland.Basegame;

public class PushPresenceFromOcean : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {

		var pushSpaces = ctx.Presence.Spaces
			.Select( x => ctx.GameState.Tokens[x])
			.Where( p => p.Space.IsOcean )
			.Distinct()
			.ToList();

		while(0 < pushSpaces.Count){
			var currentSource = pushSpaces[0];

			// #pushpresence
			var destination = await ctx.Decision( Select.Space.PushPresence( currentSource.Space, currentSource.Adjacent, Present.Always, ctx.Self.Presence.Token ));

			// apply...
			await ctx.Presence.Move( currentSource.Space, destination );

			// next
			pushSpaces.RemoveAt( 0 );
		}

	}

}