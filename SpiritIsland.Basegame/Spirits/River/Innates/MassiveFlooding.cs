namespace SpiritIsland.Basegame;

[InnatePower(MassiveFlooding.Name),Slow]
[FromSacredSite(1,Target.Invaders)]
public class MassiveFlooding {

	public const string Name = "Massive Flooding";

	[InnateOption("1 sun,2 water", "Push 1 explorer/town." )]
	static public Task Option1Async(TargetSpaceCtx ctx){
		// Push 1 Town/Explorer
		return ctx.Push(1,Human.Explorer_Town); 
	}

	[InnateOption("2 sun,3 water", "Instead, 2 Damage.  Push up to 3 explorer/town." )]
	static public async Task Option2Async(TargetSpaceCtx ctx){
		await ctx.DamageInvaders( 2 );
		await ctx.PushUpTo(3,Human.Explorer_Town);
	}

	[InnateOption("3 sun, 4 water,1 earth", "Instead, 2 Damage to each Invader" )]
	static public async Task Option3Async(TargetSpaceCtx ctx){
		await ctx.DamageEachInvader( 2 );
	}

}