namespace SpiritIsland.JaggedEarth;

public class DahanReclaimFishingGrounds : FearCardBase, IFearCard {
		
	public const string Name = "Dahan Reclaim Fishing Grounds";
	public string Text => Name;

	[FearLevel(1, "Each player chooses a different Coastal land with Dahan. In each: 1 Damage per Dahan." )]
	public Task Level1( GameState ctx ) {

		return SpiritsActOnDifferentCostalLands( ctx, 
			spaceCtx => spaceCtx.DamageInvaders( spaceCtx.Dahan.CountAll )
		);

	}

	[FearLevel(2, "Each player chooses a different Coastal land. In each: Gather up to 1 Dahan. 1 Damage per Dahan." )]
	public Task Level2( GameState ctx ) { 
		return SpiritsActOnDifferentCostalLands( ctx, 
			async spaceCtx => {
				await spaceCtx.GatherUpToNDahan( 1 );
				await spaceCtx.DamageInvaders( spaceCtx.Dahan.CountAll );
			}
		);
	}

	[FearLevel(3, "Each player chooses a different Coastal land. In each: Gather up to 1 Dahan. 2 Damage per Dahan." )]
	public Task Level3( GameState ctx ) {
		return SpiritsActOnDifferentCostalLands( ctx, 
			async spaceCtx => {
				await spaceCtx.GatherUpToNDahan( 1 );
				await spaceCtx.DamageInvaders( spaceCtx.Dahan.CountAll*2 );
			}
		);
	}

	static async Task SpiritsActOnDifferentCostalLands( GameState gs, Func<TargetSpaceCtx, Task> act ) {
		var options = ActionScope.Current.Tokens
			.Where(TerrainMapper.Current.IsCoastal)
			.Downgrade()
			.ToList();

		foreach( var spirit in gs.Spirits ) {
			if(options.Count == 0) break;
			Space space = await spirit.SelectSpaceAsync("1 damage per Dahan", options,Present.Always );
			if( space != null) {
				await act(spirit.Target(space));
				options.Remove(space);
			}
		}
	}

}