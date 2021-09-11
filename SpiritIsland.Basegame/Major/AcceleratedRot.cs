using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class AcceleratedRot {

		public const string Name = "Accelerated Rot";

		[MajorCard(AcceleratedRot.Name,4,Speed.Slow,Element.Sun,Element.Water,Element.Plant)]
		[FromPresence(2,Target.JungleOrWetland)]
		static public async Task ActAsync(TargetSpaceCtx ctx){

			// 2 fear, 4 damage
			int damageToInvaders = 4;
			ctx.AddFear(2);

			if(ctx.YouHave("3 sun,2 water,3 plant")){
				// +5 damage, remove 1 blight
				damageToInvaders += 5;
				ctx.RemoveBlight();
			}				

			await ctx.DamageInvaders(damageToInvaders);
		}

	}

}
