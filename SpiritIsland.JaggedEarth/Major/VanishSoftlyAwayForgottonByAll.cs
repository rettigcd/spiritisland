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
			var options = ctx.AllSpaces.Where( s => 4 <= ctx.Target(s).Tokens.InvaderTotal() ).ToArray();


		}
	}

	static Token[] DamagedInvaders( TargetSpaceCtx ctx ) => ctx.Tokens.Invaders().Where(t => t.Health < t.FullHealth).ToArray();


	static SpaceAction RemoveInvader => new SpaceAction(
		"Remove 1 Invader", 
		ctx => ctx.Invaders.Remove( Invader.Explorer, Invader.Town, Invader.City )
	);

	static SpaceAction RemoveExplorerOrTown => new SpaceAction( "Remove 1 Explorer/Town", ctx => ctx.Invaders.Remove( Invader.Explorer, Invader.Town ) );


	static SpaceAction RemoveAllDamagedInvaders => new SpaceAction(
		"Remove all damaged invaders",
		async ctx => { 
			var tokens = ctx.Tokens;
			foreach(var t in DamagedInvaders( ctx ))
				await tokens.Remove( t, tokens[t] );
		}
	);

}