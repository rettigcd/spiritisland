using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ManifestationOfPowerAndGlory {

		[SpiritCard( "Manifestation of Power and Glory", 3, Element.Sun, Element.Fire, Element.Air )]
		[Slow]
		[FromPresence(0,Target.Dahan)]
		static public Task Act( TargetSpaceCtx ctx ) {

			// 1 fear
			ctx.AddFear(1);

			// each dahan deals damange equal to the number of your presense in the target land
			return ctx.DamageInvaders( ctx.DahanCount * ctx.PresenceCount );

		}
	}
}
