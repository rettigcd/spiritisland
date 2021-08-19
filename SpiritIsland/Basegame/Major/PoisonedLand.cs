using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class PoisonedLand {

		[MajorCard("Poisoned Land",3,Speed.Slow,Element.Earth,Element.Plant,Element.Animal)]
		[FromPresence(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx){

			// Add 1 blight, destroy all dahan
			ctx.AddBlight(1);
			await ctx.DestroyDahan( ctx.DahanCount, DahanDestructionSource.PowerCard );

			bool hasBonus = ctx.Self.Elements.Contains("3 earth,2 plant,2 animal");
			ctx.AddFear( 1+(hasBonus?1:0) );
			await ctx.DamageInvaders( ctx.Target, 7+(hasBonus?4:0));
		}

	}
}
