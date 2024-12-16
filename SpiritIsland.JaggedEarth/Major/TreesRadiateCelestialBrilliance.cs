namespace SpiritIsland.JaggedEarth;

public class TreesRadiateCelestialBrilliance {

	const string Name = "Trees Radiate Celestial Brilliance";

	[MajorCard( Name,3,Element.Sun,Element.Moon,Element.Plant), Fast, FromPresence(1,[Filter.Jungle, Filter.NoBlight] )]
	[Instructions( "3 Fear. Defend 6. Invaders skip the next build. (In target land this turn.) -If you have- 3 Sun, 2 Moon, 2 Plant: 1 Damage per Sun you have." ), Artist( Artists.ShawnDaley )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 3 fear.
		await ctx.AddFear(3);
		// Defend 6
		ctx.Defend(6);

		// this turn, Invdaders in target land skip the next Build Action.
		ctx.Space.Skip1Build( Name );

		// if you have 3 sun 2 moon 2 plant
		if(await ctx.YouHave("3 sun,2 moon,2 plant"))
			// 1 damage per sun you have.
			await ctx.DamageInvaders( await ctx.Self.Elements.CommitToCount(Element.Sun) );
	}

}