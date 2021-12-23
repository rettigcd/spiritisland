﻿using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class ForestsOfLivingObsidian {

		[MajorCard("Forests of Living Obsidian",4,Element.Sun,Element.Fire,Element.Earth,Element.Plant), Slow, FromPresence(0)]
		[RepeatIf("2 sun,3 fire,3 earth")]
		public static async Task ActAsync(TargetSpaceCtx ctx ) {
			// add 1 badland.
			await ctx.Badlands.Add( 1 );

			// Push all dahan.
			await ctx.PushDahan( ctx.Dahan.Count );

			// 1 damage to each invader.
			await ctx.Invaders.ApplyDamageToEach(1);

			// if the original land is your sacredsite, +1 Damage to each invader
			if( ctx.Presence.IsSelfSacredSite ) // !! not exactly correct if they use a range extender
				await ctx.Invaders.ApplyDamageToEach(1);

			// if you have 2 sun 3 fire 3 earh: Repeat this power
		}

	}


}
