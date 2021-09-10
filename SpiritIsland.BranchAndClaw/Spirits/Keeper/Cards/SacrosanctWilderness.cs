﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class SacrosanctWilderness {

		// 2 fast, sun, earth, plant, 
		// range 1, no blight

		[SpiritCard("Sacrosanct Wilderness",2,Speed.Fast,Element.Sun,Element.Earth,Element.Plant)]
		[FromPresence(1,Target.NoBlight)]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// push 2 dahan
			await ctx.PushDahan( 2 );

			bool applyDamage = ctx.Tokens.Wilds()>0
				&& await ctx.Self.UserSelectsFirstText( "Select power","2 damage per wilds","add 1 wilds" );

			if( applyDamage )
				// 2 damage per wilds in target land
				await ctx.DamageInvaders( 2 * ctx.Tokens.Wilds() );
			else
				// add 1 wilds
				ctx.Tokens.Wilds().Count++;
		}


	}

}
