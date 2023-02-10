namespace SpiritIsland.Basegame;

public class PushPresenceFromOcean : GrowthActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {

		var pushSpaces = ctx.Self.Presence.ActiveSpaceStates
			.Where( p => p.Space.IsOcean )
			.Distinct()
			.ToList();

		while(0 < pushSpaces.Count){
			var currentSource = pushSpaces[0];

			// #pushpresence
			Space destination = await ctx.Decision( Select.ASpace.PushPresence( currentSource.Space, currentSource.Adjacent, Present.Always, ctx.Self.Token ));

			// apply...
			await ctx.Self.Token.Move( currentSource, destination );

			// next
			pushSpaces.RemoveAt( 0 );
		}

	}

}