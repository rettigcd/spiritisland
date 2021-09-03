using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToBloodshed {

		[MinorCard("Call to Bloodshed",1,Speed.Slow,Element.Sun,Element.Fire,Element.Animal)]
		[FromPresence(2,Target.Dahan)]
		static public Task Act(TargetSpaceCtx ctx){
			return ctx.SelectPowerOption(
				new PowerOption( "1 damage per dahan", ctx=> ctx.DamageInvaders( ctx.DahanCount ) ),
				new PowerOption( "gather up to 3 dahan", ctx => ctx.GatherUpToNDahan( 3 ) )
			);
		}
	}
}
