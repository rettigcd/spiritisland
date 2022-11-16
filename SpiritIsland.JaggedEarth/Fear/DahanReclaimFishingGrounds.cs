namespace SpiritIsland.JaggedEarth;

public class DahanReclaimFishingGrounds : IFearOptions {
		
	public const string Name = "Dahan Reclaim Fishing Grounds";
	string IFearOptions.Name => Name;

	[FearLevel(1, "Each player chooses a different Coastal land with Dahan. In each: 1 Damage per Dahan." )]
	public Task Level1( FearCtx ctx ) {

		return SpiritsActOnDifferentCostalLands( ctx, 
			spaceCtx => spaceCtx.DamageInvaders( spaceCtx.Dahan.Count )
		);

	}

	[FearLevel(2, "Each player chooses a different Coastal land. In each: Gather up to 1 Dahan. 1 Damage per Dahan." )]
	public Task Level2( FearCtx ctx ) { 
		return SpiritsActOnDifferentCostalLands( ctx, 
			async spaceCtx => {
				await spaceCtx.GatherUpToNDahan( 1 );
				await spaceCtx.DamageInvaders( spaceCtx.Dahan.Count );
			}
		);
	}

	[FearLevel(3, "Each player chooses a different Coastal land. In each: Gather up to 1 Dahan. 2 Damage per Dahan." )]
	public Task Level3( FearCtx ctx ) {
		return SpiritsActOnDifferentCostalLands( ctx, 
			async spaceCtx => {
				await spaceCtx.GatherUpToNDahan( 1 );
				await spaceCtx.DamageInvaders( spaceCtx.Dahan.Count*2 );
			}
		);
	}

	static async Task SpiritsActOnDifferentCostalLands( FearCtx ctx, Func<TargetSpaceCtx, Task> act ) {
		var options = ctx.GameState.AllActiveSpaces
			.Select( s => s.Space )
			.Where(s=>s.IsCoastal) // !!! this will miss oceans when Ocean is in play.
			.ToList();

		foreach( var spirit in ctx.Spirits ) {
			if(options.Count == 0) break;
			var spaceCtx = await spirit.SelectSpace("1 damage per Dahan", options);
			if( spaceCtx != null) {
				await act(spaceCtx);
				options.Remove(spaceCtx.Space);
			}
		}
	}

}