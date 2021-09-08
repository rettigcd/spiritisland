using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	[InnatePower("Raging Hunt",Speed.Fast)]
	[FromPresence(1,Target.NoBlight)]
	public class RagingHunt {

		[InnateOption("2 animal")]
		static public async Task Option1(TargetSpaceCtx ctx ) {
			await Gather( ctx );
			await Push( ctx );
		}

		[InnateOption( "2 plant,3 animal" )]
		static public async Task Option2( TargetSpaceCtx ctx ) {
			await Gather( ctx );
			await ctx.DamageInvaders( ctx.Tokens.Beasts().Count );
			await Push( ctx );
		}

		static Task Gather( TargetSpaceCtx ctx )
			=> ctx.GatherUpToNTokens( ctx.Target, 1, BacTokens.Beast.Generic );

		static Task Push( TargetSpaceCtx ctx )
			=> ctx.PushUpToNTokens( ctx.Target, 2, BacTokens.Beast.Generic );

	}

}
