using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class BlazingRenewal {

		[MajorCard("Blazing Renewal",5,Element.Fire,Element.Earth,Element.Plant)]
		[Fast]
		[TargetSpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// target spirit adds 2 of their destroyed presence
			int max = await ctx.Other.SelectNumber("Select # of destroyed presence to return to board", ctx.Other.Presence.Destroyed );
			if(max==0) return;

			// into a single land, up to range 2 from your presence.
			// Note - Jonah says it is the originators power and range and decision, not the targets
			TargetSpaceCtx selfPickLandCtx = await ctx.SelectTargetSpace( From.Presence, null, 2, Target.Any );

			// Add it!
			var targetSpiritOnSpace = ctx.OtherCtx.Target( selfPickLandCtx.Space );
			for(int i = 0; i < max; ++i)
				await targetSpiritOnSpace.PlaceDestroyedPresenceOnTarget();

			// if any presene was added, 2 damage to each town/city in that land.
			InvaderGroup grp = selfPickLandCtx.Invaders;
			await grp.ApplyDamageToEach(2,Invader.Town,Invader.City);

			// if you have 3 fire, 3 earth , 2 plant, 4 damage in that land
			if(ctx.YouHave( "3 fire,3 earth,2 plant"))
				await selfPickLandCtx.DamageInvaders(4);


		}

	}

}
