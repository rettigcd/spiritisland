using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class SacrosanctWilderness {

		// 2 fast, sun, earth, plant, 
		// range 1, no blight

		[SpiritCard("Sacrosanct Wilderness",2,Speed.Fast,Element.Sun,Element.Earth,Element.Plant)]
		[FromPresence(1,Target.NoBlight)]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// push 2 dahan
			await ctx.PushUpToNTokens(2, TokenType.Dahan );

			bool applyDamage = ctx.Tokens.Has(BacTokens.Wilds)
				&& await ctx.Self.UserSelectsFirstText( "Select power","2 damage per wilds","add 1 wilds" );

			if( applyDamage )
				// 2 damage per wilds in target land
				await ctx.DamageInvaders( 2 * ctx.Tokens[BacTokens.Wilds] );
			else
				// add 1 wilds
				ctx.Tokens.Adjust(BacTokens.Wilds,1);
		}


	}

}
