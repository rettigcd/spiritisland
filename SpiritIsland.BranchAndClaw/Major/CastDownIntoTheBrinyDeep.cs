using System.Xml.Linq;

namespace SpiritIsland.BranchAndClaw;

public class CastDownIntoTheBrinyDeep {
	const string Name = "Cast Down Into the Briny Deep";

	[MajorCard( Name, 9, Element.Sun, Element.Moon, Element.Water, Element.Earth )]
	[Slow]
	[FromSacredSite( 1, Target.Coastal )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// 6 fear
		ctx.AddFear(6);
		// destroy all invaders
		await ctx.Invaders.DestroyAll(Human.Invader);

		// if you have (2 sun, 2 moon, 4 water, 4 earth):
		if(await ctx.YouHave("2 sun,2 moon,4 water,4 earth"))
			await DestroyBoard( ctx, ctx.Space.Board );
	}

	static async Task DestroyBoard( SelfCtx ctx, Board board ) {
		// destroy the board containing target land and everything on that board.
		// All destroyed blight is removed from the game instead of being returned to the blight card.
		var activeSpaces = ctx.GameState.Tokens.PowerUp( board.Spaces ).ToArray();

		await DestroyTokens( ctx, activeSpaces );

		if(!ctx.Self.Text.StartsWith( "Bringer" )) { // !!! Maybe Api should have method called "Destroy Space" or "DestroyBoard"

			// destroy presence - !!! ??? Should Bringer destroy its own presence?
			foreach(var spirit in ctx.GameState.Spirits)
				foreach(var p in spirit.Presence.Placed( ctx.GameState ).Where(p=>p.Space.Board==board).ToArray() )
					await ctx.NewSelf(spirit).Presence.Destroy(p.Space, 1, DestoryPresenceCause.SpiritPower );

			// destroy board - spaces
			foreach(var space in activeSpaces)
				board.Remove( space.Space );

			if(!board.Spaces.Any())
				ctx.GameState.Island.RemoveBoard( board );

		}
		ctx.GameState.Log( new Log.LayoutChanged( $"{Name} destroyed Board {board.Name}" ) );
	}

	static async Task DestroyTokens( SelfCtx ctx, SpaceState[] spaces ) {

		foreach(SpaceState space in spaces) {

			var targetCtx = ctx.Target(space); // !!! switch this to using ActionableSpaceState once Bringer implements it.

			// Destroy Invaders
			await targetCtx.Invaders.DestroyAll( Human.Invader );

			// Destroy Dahan
			await targetCtx.Dahan.DestroyAll();

			if(!ctx.Self.Text.StartsWith("Bringer")) // !!!
				// Destroy all other tokens
				foreach(var token in space.Keys.OfType<IVisibleToken>().ToArray())
					await targetCtx.Tokens.Destroy( token, space[token] );

		}
	}

}