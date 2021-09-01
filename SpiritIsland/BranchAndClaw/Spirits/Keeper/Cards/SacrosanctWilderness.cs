using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class SacrosanctWilderness {

		// 2 fast, sun, earth, plant, 
		// range 1, no blight

		[SpiritCard("Sacrosanct Wilderness",2,Speed.Fast,Element.Sun,Element.Earth,Element.Plant)]
		[FromPresence(1,Target.NoBlight)]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			var gsbac = ctx.GameState as GameState_BranchAndClaw;

			// push 2 dahan
			await ctx.PowerPushUpToNDahan(2);

			bool applyDamage = gsbac.Wilds.AreOn( ctx.Target ) 
				&& await ctx.Self.UserSelectsFirstText( "Select power","2 damage per wilds","add 1 wilds" );

			if( applyDamage )
				// 2 damage per wilds in target land
				await ctx.DamageInvaders( 2 * gsbac.Wilds.GetCount(ctx.Target) );
			else
				// add 1 wilds
				gsbac.Wilds.AddOneTo( ctx.Target );
		}


	}

}
