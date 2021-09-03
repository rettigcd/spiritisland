using SpiritIsland;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( GatherTheWarriors.Name,Speed.Slow)]
	[FromPresence(1)]
	public class GatherTheWarriors {

		public const String Name = "Gather the Warriors";

		[InnateOption(Element.Animal)]
		static public async Task OptionAsync(TargetSpaceCtx ctx ) {
			var elements = ctx.Self.Elements;
			int gatherCount = elements[Element.Air];
			int pushCount = elements[Element.Sun];

			await ctx.GatherUpToNTokens(ctx.Target,gatherCount, TokenType.Dahan );
			await ctx.PushUpToNTokens(pushCount, TokenType.Dahan );
		}


	}

}
