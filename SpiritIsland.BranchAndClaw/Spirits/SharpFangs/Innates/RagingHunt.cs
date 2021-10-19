using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	[InnatePower("Raging Hunt"),Fast]
	[FromPresence(1,Target.NoBlight)]
	public class RagingHunt {

		[InnateOption("2 animal","You may Gather 1 beast.")]
		// !!! secondary, You may Push up to 2 beast
		static public async Task Option1(TargetSpaceCtx ctx ) {
			await Gather( ctx );
			await Push( ctx );
		}

		[InnateOption( "2 plant,3 animal", "1 Damage per beast." )]
		static public async Task Option2( TargetSpaceCtx ctx ) {
			await Gather( ctx );
			await ctx.DamageInvaders( ctx.Beasts.Count );
			await Push( ctx );
		}

		[InnateOption("2 animal","You may Push up to 2 beast.", AttributePurpose.DisplayOnly)]
		static public void Noop( TargetSpaceCtx _ ) { }

		static Task Gather( TargetSpaceCtx ctx )
			=> ctx.GatherUpTo( 1, TokenType.Beast.Generic );

		static Task Push( TargetSpaceCtx ctx )
			=> ctx.PushUpTo( ctx.Space, 2, TokenType.Beast.Generic );

	}

}
