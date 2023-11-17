namespace SpiritIsland.NatureIncarnate;

[InnatePower(Name,"If Incarna is empowered, you may Repeat this Power once each turn."),Slow]
[FromIncarna]
public class SmashStompAndFlatten {

	public const string Name = "Smash, Stomp, and Flatten";

	[InnateTier("2 fire,1 earth", "2 Damage.", 0 )]
	static public Task Option1Async(TargetSpaceCtx ctx){
		return ctx.DamageInvaders(2);
	}

	[InnateTier( "3 fire,1 earth,1 plant", "1 Damage. Push 1 Dahan.", 0 )]
	static public async Task Option2Async(TargetSpaceCtx ctx){
		await ctx.DamageInvaders( 1+2 );
		await ctx.PushUpTo(1,Human.Dahan);
	}

	[InnateTier( "4 fire,2 earth,1 plant", "1 Fear. 1 Damage.", 1 )]
	static public async Task Option3Async(TargetSpaceCtx ctx){
		ctx.AddFear(1);
		await ctx.DamageInvaders( 1 );
	}

	[InnateTier( "5 fire,2 earth,2 plant", "2 Damage. 2 Damage to Dahan.", 2 )]
	static public async Task Option4Async( TargetSpaceCtx ctx ) {
		await ctx.DamageEachInvader( 2 );
		await ctx.DamageDahan( 2 );
	}

}