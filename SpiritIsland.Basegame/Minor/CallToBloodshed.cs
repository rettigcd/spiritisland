using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToBloodshed {

		[MinorCard("Call to Bloodshed",1,Speed.Slow,Element.Sun,Element.Fire,Element.Animal)]
		[FromPresence(2,Target.Dahan)]
		static public Task Act(TargetSpaceCtx ctx){
			return ctx.SelectActionOption(
				new ActionOption( "1 damage per dahan", () => ctx.DamageInvaders( ctx.DahanCount ) ),
				new ActionOption( "gather up to 3 dahan", () => ctx.GatherUpToNDahan( 3 ) )
			);
		}
	}
}
