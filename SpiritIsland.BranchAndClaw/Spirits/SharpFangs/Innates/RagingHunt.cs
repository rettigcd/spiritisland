using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	[InnatePower("Raging Hunt"),Fast]
	[FromPresence(1,Target.NoBlight)]
	public class RagingHunt {

		[InnateOption("2 animal","You may Gather 1 beast.",0)]
		static public Task Gather(TargetSpaceCtx ctx ) => ctx.GatherUpTo( 1, TokenType.Beast.Generic );

		[InnateOption( "2 plant,3 animal", "1 Damage per beast.",1)]
		static public Task Damage( TargetSpaceCtx ctx ) => ctx.DamageInvaders( ctx.Beasts.Count );

		[InnateOption("2 animal","You may Push up to 2 beast.", 2)]
		static public void Noop( TargetSpaceCtx ctx ) => ctx.PushUpTo( 2, TokenType.Beast.Generic );

	}

}
