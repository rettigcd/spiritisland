using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CropsWitherAndFade {

		[SpiritCard("Crops Wither and Fade",1,Speed.Slow,Element.Moon,Element.Fire,Element.Plant)]
		[FromPresence(0)]
		static public Task ActAsync( TargetSpaceCtx ctx ){

			// 2 fear
			ctx.AddFear( 2 );

			return ctx.SelectActionOption(
				new ActionOption("replace town with explorer", () => Replace.Downgrade(ctx,Invader.Town)),
				new ActionOption("replace city with town", () => Replace.Downgrade(ctx,Invader.City))
			);

		}

	}

}
