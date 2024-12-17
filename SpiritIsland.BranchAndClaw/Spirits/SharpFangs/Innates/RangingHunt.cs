namespace SpiritIsland.BranchAndClaw;

[InnatePower(Name),Fast]
[FromPresence(1,Filter.NoBlight)]
public class RangingHunt {

	public const string Name = "Raging Hunt";

	[InnateTier("2 animal","You may Gather 1 beast.",0)]
	static public Task Gather(TargetSpaceCtx ctx ) => ctx.GatherUpTo( 1, Token.Beast );

	[InnateTier( "2 plant,3 animal", "1 Damage per beast.",1)]
	static public Task Damage( TargetSpaceCtx ctx ) => ctx.DamageInvaders( ctx.Beasts.Count );

	[InnateTier("2 animal","You may Push up to 2 beast.", 2)]
	static public Task Noop( TargetSpaceCtx ctx ) => ctx.PushUpTo( 2, Token.Beast );

}

// !! Is there a better way to change this? without creating a new class
[InnatePower(Name), Fast]
[FromPresence(1)]
public class RangingHuntOnBlight {

	// Unconstrained by Ravaged Lands
	// Ranging Hunt(your left Innate Power) may target lands with Blight.

	public const string Name = "Raging Hunt";

	[InnateTier("2 animal", "You may Gather 1 beast.", 0)]
	static public Task Gather(TargetSpaceCtx ctx) => ctx.GatherUpTo(1, Token.Beast);

	[InnateTier("2 plant,3 animal", "1 Damage per beast.", 1)]
	static public Task Damage(TargetSpaceCtx ctx) => ctx.DamageInvaders(ctx.Beasts.Count);

	[InnateTier("2 animal", "You may Push up to 2 beast.", 2)]
	static public Task Noop(TargetSpaceCtx ctx) => ctx.PushUpTo(2, Token.Beast);

}
