namespace SpiritIsland.BranchAndClaw;

[InnatePower("Raging Hunt"),Fast]
[FromPresence(1,Target.NoBlight)]
public class RagingHunt {

	[InnateTier("2 animal","You may Gather 1 beast.",0)]
	static public Task Gather(TargetSpaceCtx ctx ) => ctx.GatherUpTo( 1, Token.Beast );

	[InnateTier( "2 plant,3 animal", "1 Damage per beast.",1)]
	static public Task Damage( TargetSpaceCtx ctx ) => ctx.DamageInvaders( ctx.Beasts.Count );

	[InnateTier("2 animal","You may Push up to 2 beast.", 2)]
	static public Task Noop( TargetSpaceCtx ctx ) => ctx.PushUpTo( 2, Token.Beast );

}