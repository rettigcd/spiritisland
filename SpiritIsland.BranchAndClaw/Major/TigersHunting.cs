namespace SpiritIsland.BranchAndClaw;

public class TigersHunting {

	[MajorCard("Tigers Hunting", 2, Element.Sun, Element.Moon, Element.Animal)]
	[Fast]
	[FromPresenceIn(1,Terrain.Jungle,Target.NoBlight)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 2 fear
		ctx.AddFear(2);

		// add 1 beast.
		var beasts = ctx.Beasts;
		await beasts.Add(1);

		// Gather up to 1 beast.
		await ctx.GatherUpTo(1,Token.Beast);

		// 1 damage per beast.
		await ctx.DamageInvaders( beasts.Count );

		// Push up to 2 beast
		await ctx.PushUpTo(2, Token.Beast);

		// if you have 2 sun 2 moon 3 animal
		if(await ctx.YouHave("2 sun,2 moon,3 animal")) {
			//   1 damage in adjacent land without blight,
			//   and +1 damage per beast there
			var noBlight = await ctx.SelectAdjacentLand( "1 Damage in land w/o blight", ctx=>!ctx.HasBlight);
			if(noBlight != null)
				await noBlight.DamageInvaders(1 + noBlight.Beasts.Count );
		}

	}

}