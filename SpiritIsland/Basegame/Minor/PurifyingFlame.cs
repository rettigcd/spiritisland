using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class PurifyingFlame {

		[MinorCard("Purifying Flame",1,Speed.Slow,Element.Sun,Element.Fire,Element.Air,Element.Plant)]
		[FromSacredSite(1)]
		static public async Task Act(TargetSpaceCtx ctx){

			// on target, spirit should be able to do one or both
			bool canRemoveBlight = ctx.IsOneOf( Terrain.Mountain, Terrain.Sand );
			bool doDamage = ctx.HasInvaders 
				&& (!canRemoveBlight || await ctx.Self.SelectFirstText( "Select option", "damageInvaders", "remove blight" ) );
			if(doDamage)
				// 1 damage per blight
				await ctx.DamageInvaders(ctx.BlightOnSpace);
			else
				// if target land is M/S, you may INSTEAD remove 1 blight
				ctx.RemoveBlight();

		}

	}

}
