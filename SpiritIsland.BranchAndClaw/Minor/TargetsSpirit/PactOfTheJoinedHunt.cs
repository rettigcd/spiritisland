using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class PactOfTheJoinedHunt {

		[MinorCard( "Pact of the Joined Hunt", 1, Speed.Slow, Element.Sun, Element.Plant, Element.Animal )]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {
			// Target spirit gathers 1 dahan into one of their lands
			var spaceCtx = await ctx.OtherCtx.TargetLandWithPresence("Gather 1 dahan to");
			await spaceCtx.GatherUpToNDahan(1);

			// 1 damage in that land per dahan present
			await spaceCtx.DamageInvaders( spaceCtx.DahanCount );

		}

	}
}
