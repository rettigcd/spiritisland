using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class TheWoundedWildReturnsOnItsAssailants {

		[MajorCard("The Wounded Wild Turns on its Assailants",4,Element.Fire,Element.Plant,Element.Animal), Slow, FromPresence(1,Target.Blight)]
		public static async Task ActAsync(TargetSpaceCtx ctx ) {
			// Add 2 badlands
			await ctx.Badlands.Add( 2 );

			// Gather up to 2 beast
			await ctx.GatherUpTo(2,TokenType.Beast);

			// (watch for invaders destoryed in this land)
			int destroyed = 0;
			// the only way a token will be removed, is if it is destroyed - !!! single player mode only
			ctx.GameState.Tokens.TokenRemoved.ForRound.Add( ( gs, args ) => destroyed++ );

			// 1 damamge per blight/beast/wilds.
			await ctx.DamageInvaders( ctx.Blight.Count + ctx.Beasts.Count + ctx.Wilds.Count );

			// if you have 2 fire, 3 air, 2 animal
			if( await ctx.YouHave("2 fire,3 air,2 animal"))
				// 2 fear per invader destroyed by this Power (max 8 fear)
				ctx.AddFear( System.Math.Min( 8, destroyed*2));

		}

	}


}
