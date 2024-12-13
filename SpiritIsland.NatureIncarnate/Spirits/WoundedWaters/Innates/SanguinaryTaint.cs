namespace SpiritIsland.NatureIncarnate;

[InnatePower( Name )]
[Slow,FromPresence( 1 )]
public class SanguinaryTaint {

	public const string Name = "Sanguinary Taint";

	[InnateTier( "2 animal", "1 Fear. 1 Damage. Push 1 Dahan." )]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		await ctx.AddFear(1);
		await ctx.DamageInvaders(1);
		await ctx.PushDahan(1);
	}

	[InnateTier( "1 water,3 animal", "1 Damage. Add 1 Beast." )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		await ctx.AddFear(1);
		await ctx.DamageInvaders( 2 );
		await ctx.PushDahan( 1 );
		await ctx.Beasts.AddAsync( 1 );
	}

	[InnateTier( "2 fire,2 water,5 animal", "1 Fear. 4 Damage. Add 1 Disease" )]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		await ctx.AddFear( 2 );
		await ctx.DamageInvaders( 6 );
		await ctx.PushDahan( 1 );
		await ctx.Beasts.AddAsync( 1 );
		await ctx.Disease.AddAsync(1);
	}

}
