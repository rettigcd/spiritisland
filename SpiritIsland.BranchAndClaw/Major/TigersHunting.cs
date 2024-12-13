using SpiritIsland.A;

namespace SpiritIsland.BranchAndClaw;

public class TigersHunting {

	[MajorCard("Tigers Hunting", 2, Element.Sun, Element.Moon, Element.Animal),Fast,FromPresence(Filter.Jungle,1,Filter.NoBlight)]
	[Instructions( "2 Fear. Add 1 Beasts. Gather up to 1 Beasts. 1 Damage per Beasts. Push up to 2 Beasts. -If you have- 2 Sun, 2 Moon, 3 Animal: 1 Damage in an adjacent land without Blight, and +1 Damage per Beasts there." ), Artist( Artists.CariCorene )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 2 fear
		await ctx.AddFear(2);

		// add 1 beast.
		var beasts = ctx.Beasts;
		await beasts.AddAsync(1);

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
			Space noBlight = await ctx.Self.SelectAsync( new A.SpaceDecision( 
				"1 Damage in land w/o blight", 
				ctx.Space.Adjacent.Where( tokens => !tokens.Blight.Any ), 
				Present.Always 
			) );
			if(noBlight != null)
				await ctx.Target(noBlight).DamageInvaders(1 + noBlight.Beasts.Count );
		}

	}

}