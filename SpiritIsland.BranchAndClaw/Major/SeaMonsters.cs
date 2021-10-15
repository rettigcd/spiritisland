using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SeaMonsters {

		[MajorCard( "Sea Monsters", 5, Element.Water, Element.Animal )]
		[Slow]
		[FromPresence( 1, Target.CoastalOrWetlands )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			await DoPowerAction( ctx );

			// if you have 3 water and 3 animal: repeat this power
			if(ctx.YouHave("3 water,3 animal"))
				await DoPowerAction( ctx );
		}

		static async Task DoPowerAction( TargetSpaceCtx ctx ) {
			// add 1 beast.
			var beasts = ctx.Beasts;
			beasts.Count++;

			// IF invaders are present,
			if(ctx.HasInvaders)
				// 2 fear per beast (max 8 fear).
				ctx.AddFear( System.Math.Min( 8, beasts.Count * 2 ) );

			int damage = 3 * beasts.Count // 3 damage per beast
				+ ctx.Blight.Count; // 1 damage per blight
			await ctx.DamageInvaders( damage );
		}
	}


}
