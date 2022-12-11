namespace SpiritIsland.BranchAndClaw;

public class DahanAttack : IFearCard {

	public const string Name = "Dahan Attack";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }

	[FearLevel( 1, "Each player removes 1 eplorer from a land with dahan" )]
	public async Task Level1( GameCtx ctx ) {

		// Each player removes 1 eplorer from a land with dahan
		foreach(var spirit in ctx.Spirits)
			await spirit.RemoveTokenFromOneSpace(ctx.Lands( SpaceFilters.WithDahanAndExplorers).Select(x=>x.Space),1,Invader.Explorer);

	}

	[FearLevel( 2, "Each player chooses a different land with dahan.  1 damage per dahan there" )]
	public async Task Level2( GameCtx ctx ) {

		// Each player chooses a different land with dahan.  1 damage per dahan there
		var options = ctx.Lands( SpaceFilters.WithDahanAndInvaders).Select( x => x.Space ).ToList();

		foreach(var spirit in ctx.Spirits)
			options.Remove( await DamagePerDahanOnOne( options, spirit ) );
			
	}

	[FearLevel( 3, "each player chooses a different land with towns/cities.  Gather 1 dahan into that land.  Then 2 damage per dahan there" )]
	public async Task Level3( GameCtx ctx ) {


		// each player chooses a different land with towns/cities.  
		var options = ctx.GameState.AllActiveSpaces
			.Where( s => s.HasAny(Invader.Town,Invader.City) )
			.Select( x => x.Space )
			.ToList();

		foreach(var spirit in ctx.Spirits) {
			var spaceCtx = await spirit.SelectSpace( "Remove 1 explorer", options );
			if(spaceCtx != null) {
				options.Remove( spaceCtx.Space );
				// Gather 1 dahan into that land.
				await spaceCtx.GatherDahan( 1 );
				// Then 2 damage per dahan there
				await spaceCtx.DamageInvaders( 2*spaceCtx.Dahan.Count );
			}
		}
	}

	static async Task<Space> DamagePerDahanOnOne( List<Space> options, SelfCtx spirit ) {
		var spaceCtx = await spirit.SelectSpace( "1 damage per dahan", options );
		if(spaceCtx != null) {
			await spaceCtx.DamageInvaders( spaceCtx.Dahan.Count );
			return spaceCtx.Space;
		}
		return null;
	}

}
