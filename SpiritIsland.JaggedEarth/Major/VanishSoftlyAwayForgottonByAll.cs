namespace SpiritIsland.JaggedEarth;
public class VanishSoftlyAwayForgottonByAll {

	[MajorCard("Vanish Softly Away, Forgotten by All",3,Element.Moon,Element.Air), Slow, FromPresence(2)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {

		await ctx.SelectActionOption(
			new SpaceAction(
				"Remove 1 invader. Remove 1 explorer/town",
				async ctx => { 
					await RemoveInvader.Execute(ctx);
					await RemoveExplorerOrTown.Execute(ctx);
				}
			),
		// OR
			RemoveAllDamagedInvaders
		);

		// adversary or Scenarios rules that prevent or alter Remvoal do not affect this Power.

		// if you have 3 moon, 3 air:
		if( await ctx.YouHave("3 moon,3 air" )) {
			// in any 2 lands with 4 or more invaders: remove 1 invader
			var options = ctx.GameState.AllActiveSpaces.Where( s => 4 <= s.InvaderTotal() ).ToArray();
			// !!! implement
		}
	}

	static IToken[] DamagedInvaders( TargetSpaceCtx ctx ) => ctx.Tokens.InvaderTokens().Where(t => t.RemainingHealth < t.FullHealth).ToArray();


	static SpaceAction RemoveInvader => new SpaceAction(
		"Remove 1 Invader", 
		ctx => ctx.Invaders.RemoveLeastDesirable( Human.Invader )
	);

	static SpaceAction RemoveExplorerOrTown => new SpaceAction( "Remove 1 Explorer/Town", 
		ctx => ctx.Invaders.RemoveLeastDesirable( Human.Explorer_Town )
	);


	static SpaceAction RemoveAllDamagedInvaders => new SpaceAction(
		"Remove all damaged invaders",
		async ctx => { 
			var tokens = ctx.Tokens;
			foreach(var t in DamagedInvaders( ctx ))
				await ctx.Remove( t, tokens[t] );
		}
	);

}