namespace SpiritIsland.BranchAndClaw;

public class Discord : FearCardBase, IFearCard {

	public const string Name = "Discord";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 strife in a different land with at least 2 invaders" )]
	public async Task Level1( GameCtx ctx ) {
		var options = LandsWith2Invaders( ctx );

		// each player adds 1 strife in a different land with at least 2 invaders
		foreach(SelfCtx spirit in ctx.Spirits)
			options.Remove( await spirit.AddStrifeToOne(options) );

	}

	[FearLevel( 2, "Each player adds 1 strife in a different land with at least 2 invaders. Then each invader takes 1 damage per strife it has." )]
	public async Task Level2( GameCtx ctx ) {
		var options = LandsWith2Invaders( ctx );

		// each player adds 1 strife in a different land with at least 2 invaders
		foreach(SelfCtx spirit in ctx.Spirits)
			options.Remove( await spirit.AddStrifeToOne( options ) );

		// Then each invader takes 1 damage per strife it has.
		await StrifedRavage.StrifedInvadersTakeDamagePerStrife( ctx );
	}

	[FearLevel( 3, "each player adds 1 strife in a different land with at least 2 invaders. Then, each invader with strife deals damage to other invaders in that land." )]
	public async Task Level3( GameCtx ctx ) {

		var options = LandsWith2Invaders( ctx );

		// each player adds 1 strife in a different land with at least 2 invaders.
		foreach(SelfCtx spirit in ctx.Spirits) {
			var spaceCtx = await spirit.SelectSpace( "Add strife", options.Select(x=>x.Space) );
			if(spaceCtx != null) {
				await spaceCtx.AddStrife();
				options.Remove( spaceCtx.Tokens );

				// Then, each invader with strife deals damage to other invaders in that land.
				await StrifedRavage.StrifedInvadersDealsDamageToOtherInvaders.Execute( spaceCtx );
			}
		}
	}

	static List<SpaceState> LandsWith2Invaders( GameCtx ctx ) {
		return ctx.GameState.AllActiveSpaces
			.Where( s => 2 <= s.InvaderTotal() )
			.ToList();
	}

}