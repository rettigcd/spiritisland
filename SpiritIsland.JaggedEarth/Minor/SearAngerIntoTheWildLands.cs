using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class SearAngerIntoTheWildLands{ 
		[MinorCard("Sear Anger into the Wild Lands",0,Element.Sun,Element.Fire,Element.Plant),Slow,FromPresence(1)]
		static public Task ActAsync( TargetSpaceCtx ctx ){
			return ctx.SelectActionOption(
				Cmd.Add1Badlands,
				// If wilds and Invaders are present, 1 fear and 1 Damage.
				(ctx.Wilds.Any && ctx.HasInvaders) ? FearAndDamage : null
			);
		}

		static SpaceAction FearAndDamage => new SpaceAction("1 fear and 1 Damage", async ctx => { ctx.AddFear(1); await ctx.DamageInvaders(1); } );
		
	}

}
