using System.Xml.Linq;

namespace SpiritIsland.BranchAndClaw;

public class CastDownIntoTheBrinyDeep {
	const string Name = "Cast Down Into the Briny Deep";
	// https://querki.net/raw/darker/spirit-island-faq/Cast+Down+into+the+Briny+Deep

	[MajorCard( Name, 9, Element.Sun, Element.Moon, Element.Water, Element.Earth )]
	[Slow]
	[FromSacredSite( 1, Target.Coastal )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// 6 fear
		ctx.AddFear(6);
		// destroy all invaders
		await ctx.Invaders.DestroyAll(Human.Invader);

		// if you have (2 sun, 2 moon, 4 water, 4 earth):
		if(await ctx.YouHave("2 sun,2 moon,4 water,4 earth" )) {
			// Pick board
			// (from querki, if space has multiple boards, user selects.)
			var boards = ctx.Space.Boards;
			var options = boards.Select(b=>b.Name).Order().ToArray();
			string name = await ctx.Self.SelectText("Pick Board To Destroy",options,Present.AutoSelectSingle);
			var board = boards.Single(b=>b.Name == name);

			// Destroy it
			await DestroyBoard( ctx, board );
		}
	}

	static async Task DestroyBoard( SelfCtx ctx, Board board ) {
		// destroy the board containing target land and everything on that board.
		// All destroyed blight is removed from the game instead of being returned to the blight card.
		var activeSpaces = board.Spaces.Tokens().ToArray();

		await DestroyTokens( ctx, activeSpaces );

		if(!ctx.Self.Text.StartsWith( "Bringer" )) { // !!! Maybe Api should have method called "Destroy Space" or "DestroyBoard"
			// destroy board - spaces
			foreach(SpaceState space in activeSpaces)
				board.Remove( space.Space );

			if(!board.Spaces.Any())
				ctx.GameState.Island.RemoveBoard( board );

		}
		ctx.GameState.Log( new Log.LayoutChanged( $"{Name} destroyed Board {board.Name}" ) );
	}

	static async Task DestroyTokens( SelfCtx ctx, SpaceState[] spaces ) {

		foreach(SpaceState space in spaces) {

			var targetCtx = ctx.Target(space);

			// Destroy Invaders
			await targetCtx.Invaders.DestroyAll( Human.Invader );

			// Destroy Dahan
			await targetCtx.Dahan.DestroyAll();

			if(!ctx.Self.Text.StartsWith("Bringer")) // !!!
				// Destroy all other tokens
				foreach(IToken token in space.Keys.OfType<IToken>().ToArray())
					await targetCtx.Tokens.Destroy( token, space[token] );
		}
	}

}