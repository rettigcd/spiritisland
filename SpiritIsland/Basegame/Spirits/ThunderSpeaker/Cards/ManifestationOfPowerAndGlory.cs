using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ManifestationOfPowerAndGlory {

		[SpiritCard( "Manifestation of Power and Glory", 3, Speed.Slow, Element.Sun, Element.Fire, Element.Air )]
		[FromPresence(0,Target.Dahan)]
		static public async Task Act( TargetSpaceCtx ctx ) {
			var target = ctx.Target;
			// 1 fear
			ctx.AddFear(1);
			// each dahan deals damange equal to the number of your presense in the target land
			int dahan = ctx.GameState.DahanCount( target );
			int presence = ctx.Self.Presence.On(target);
			await ctx.DamageInvaders(target, dahan*presence);
		}
	}
}
