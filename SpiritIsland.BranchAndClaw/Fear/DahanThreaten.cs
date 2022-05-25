namespace SpiritIsland.BranchAndClaw;

public class DahanThreaten : IFearOptions {

	public const string Name = "Dahan Threaten";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "each player adds 1 strife in a land with dahan" )]
	public async Task Level1( FearCtx ctx ) {

		// each player adds 1 strife in a land with dahan
		foreach(SelfCtx spirit in ctx.Spirits)
			await spirit.AddStrifeToOne( ctx.Lands(ctx.WithDahan) );
	}

	[FearLevel( 2, "each player adds 1 strife in a land with dahan. For the rest of t his turn, invaders have -1 health per strife to a minimum of 1" )]
	public async Task Level2( FearCtx ctx ) {

		var actionId = Guid.NewGuid();

		// each player adds 1 strife in a land with dahan
		foreach(SelfCtx spirit in ctx.Spirits)
			await spirit.AddStrifeToOne( ctx.Lands(ctx.WithDahan) );

		// For the rest of this turn, invaders have -1 health per strife to a minimum of 1
		await StrifedRavage.InvadersReduceHealthByStrifeCount( ctx.GameState, actionId );

	}

	[FearLevel( 3, "Each player adds 1 strife in a land with dahan.  In every land with strife, 1 damage per dahan" )]
	public async Task Level3( FearCtx ctx ) {

		// each player adds 1 strife in a land with dahan
		foreach(SelfCtx spirit in ctx.Spirits)
			await spirit.AddStrifeToOne( ctx.Lands(ctx.WithDahan) );

		var decidingSpirit = ctx.Spirits.First();
		// in every land with strife, 1 damage per dahan

		foreach(var space in ctx.LandsWithStrife()) {
			var spaceCtx = decidingSpirit.Target(space);
			await spaceCtx.DamageInvaders( spaceCtx.Dahan.Count );
		}
	}

}