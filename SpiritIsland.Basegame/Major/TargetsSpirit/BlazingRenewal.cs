using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class BlazingRenewal {

		[MajorCard("Blazing Renewal",5,Element.Fire,Element.Earth,Element.Plant)]
		[Fast]
		[AnySpirit]
		static public async Task ActAsync( TargetSpiritCtx ctx ) {

			// into a single land, up to range 2 from your presence.
			// Note - Jonah says it is the originators power and range and decision, not the targets
			var spaceOptions = ctx.Self.TargetLandApi.GetTargetOptions(ctx.Self,ctx.GameState,From.Presence,null,2,Target.Any)
				.Where(ctx.Other.Presence.IsValid)
				.ToArray();
			TargetSpaceCtx selfPickLandCtx = await ctx.SelectSpace("Select location for target spirit to add presence", spaceOptions);

			var otherCtx = ctx.OtherCtx;
			bool additionalDamage = await ctx.YouHave("3 fire,3 earth,2 plant");


			// target spirit adds 2 of their destroyed presence
			int max = await otherCtx.Self.SelectNumber("Select # of destroyed presence to place on " + selfPickLandCtx.Space.Label, otherCtx.Self.Presence.Destroyed );
			if(max==0) return;

			// Add it!
			TargetSpaceCtx targetSpiritOnSpace = otherCtx.Target( selfPickLandCtx.Space );
			for(int i = 0; i < max; ++i)
				await targetSpiritOnSpace.Presence.PlaceDestroyedHere();

			// if any presene was added, 2 damage to each town/city in that land.
			await selfPickLandCtx.DamageEachInvader(2,Invader.Town,Invader.City);

			// if you have 3 fire, 3 earth , 2 plant, 4 damage in that land
			if(additionalDamage)
				await selfPickLandCtx.DamageInvaders(4);

		}

	}

}
