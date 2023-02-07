namespace SpiritIsland.JaggedEarth;

public class TreesRadiateCelestialBrilliance {

	const string Name = "Trees Radiate Celestial Brilliance";

	[MajorCard( Name,3,Element.Sun,Element.Moon,Element.Plant), Fast, FromPresence(1,Target.Jungle, Target.NoBlight )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 3 fear.
		ctx.AddFear(3);
		// Defend 6
		ctx.Defend(6);

		// this turn, Invdaders in target land skip the next Build Action.
		ctx.Tokens.Skip1Build( Name );

		// if you have 3 sun 2 moon 2 plant
		if(await ctx.YouHave("3 sun,2 moon,2 plant"))
			// 1 damage per sun you have.
			await ctx.DamageInvaders( ctx.Self.Elements[Element.Sun] );
	}

}