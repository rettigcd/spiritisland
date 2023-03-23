namespace SpiritIsland.Basegame;

public class OvergrowInANight {

	[SpiritCard( "Overgrow in a Night", 2, Element.Moon, Element.Plant ), Fast, FromPresence( 1 )]
	[Instructions("Add 1 Presence. -or- If target land has your Presence and Invaders, 3 Fear."), Artist( Artists.JorgeRamos )]
	static public Task ActionAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction("Add 1 presence", async ctx => {
				var from = await ctx.Self.SelectMovablePresence();
				await ctx.Self.Presence.Place( from, ctx.Space );
			} ),
			new SpaceAction( "3 fear", ctx => ctx.AddFear(3) )
				.OnlyExecuteIf( x=>x.Presence.IsHere && x.Tokens.HasInvaders() ) // if presence and invaders
		);

	}

}