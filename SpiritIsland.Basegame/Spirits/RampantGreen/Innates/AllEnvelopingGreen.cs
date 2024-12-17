namespace SpiritIsland.Basegame;

[InnatePower(Name),Fast]
[FromSacredSite(1)]
public class AllEnvelopingGreen {

	public const string Name = "All Enveloping Green";

	[InnateTier("1 water,3 plant","Defend 2.")]
	static public Task Defend2( TargetSpaceCtx ctx ) {
		//defend 2
		ctx.Defend(2);
		return Task.CompletedTask;
	}

	[InnateTier( "2 water,4 plant","Instead, Defend 4." )]
	static public Task Defend4( TargetSpaceCtx ctx ) {
		//defend 4 (instead)
		ctx.Defend(4);
		return Task.CompletedTask;
	}

	[InnateTier( "3 water,5 plant,1 earth", "Also, remove 1 blight." )]
	static public Task AndRemove1Blight( TargetSpaceCtx ctx ) {
		Defend4(ctx);
		// also remove 1 blight
		return ctx.RemoveBlight();
	}

}