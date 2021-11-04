using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("The Teeming Host Arrives"), Fast, FromPresence(2)]
	public class TheTeemingHostArrives {

		// 2 air 1 animal - gather up to 1 beast
		[InnateOption("2 air,1 animal","Gather up to 1 beast")]
		static public Task Option1( TargetSpaceCtx ctx ) {
			return ctx.GatherUpTo(1,TokenType.Beast.Generic);
		}

		// 3 air 1 water 2 animal - Instead, Gather up to 1 beast per air you have.
		[InnateOption("3 air,1 water,2 animal","Instead, Gather up to 1 beast per air you have.")]
		static public Task Option2( TargetSpaceCtx ctx ) {
			return ctx.GatherUpTo(ctx.Self.Elements[Element.Air],TokenType.Beast.Generic);
		}

		// 1 fire 4 air 2 animal - push up to 3 beast
		[InnateOption("1 fire,4 air,2 animal","Push up to 3 beast.",1)]
		static public Task Option3( TargetSpaceCtx ctx ) {
			return ctx.PushUpTo(3,TokenType.Beast.Generic);
		}

	}


}
