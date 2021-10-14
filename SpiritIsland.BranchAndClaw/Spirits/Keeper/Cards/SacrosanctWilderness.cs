using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class SacrosanctWilderness {

		// 2 fast, sun, earth, plant, 
		// range 1, no blight

		[SpiritCard("Sacrosanct Wilderness",2,Element.Sun,Element.Earth,Element.Plant)]
		[Fast]
		[FromPresence(1,Target.NoBlight)]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// push 2 dahan
			await ctx.PushDahan( 2 );

			await ctx.SelectActionOption(
				new ActionOption("2 Damage per wilds", () => ctx.DamageInvaders( 2 * ctx.Tokens.Wilds ), ctx.Tokens.Has(TokenType.Wilds) ),
				new ActionOption("Add 1 wilds", () => ctx.Tokens.Wilds.Count++)
			);

		}


	}

}
