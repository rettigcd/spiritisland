using System;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( GatherTheWarriors.Name ),SlowButFastIf("4 air")]
	[FromPresence(1)]
	public class GatherTheWarriors {

		public const String Name = "Gather the Warriors";

		[InnateOption( "4 air", "This Power may be fast.", AttributePurpose.DisplayOnly )]
		static public Task MayBeFastAsync(TargetSpaceCtx _ ) { return null; }


		[InnateOption( "1 animal", "Gather up to 1 dahan per air you have. Push up to 1 dahan per sun you have." )]
		static public async Task OptionAsync(TargetSpaceCtx ctx ) {
			var elements = ctx.Self.Elements;
			int gatherCount = elements[Element.Air];
			int pushCount = elements[Element.Sun];

			await ctx.GatherUpToNDahan( gatherCount );
			await ctx.PushUpToNDahan( pushCount );
		}


	}

}
