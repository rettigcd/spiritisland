using SpiritIsland.Select;

namespace SpiritIsland.JaggedEarth;

public class TransformToAMurderousDarkness {

	[MajorCard("Transform to a Murderous Darkness",6,Element.Moon,Element.Fire,Element.Air,Element.Water,Element.Plant), Slow, AnySpirit]
	[Instructions( "Target Spirit may choose one of their Sacred Sites. In that land: Replace all their Presence with Badlands; the replaced Presence leave the game. Push any number of those Badlands. 3 Fear. 3 Damage per Presence replaced. -If you have- 3 Moon, 2 Fire, 2 Air: 1 Damage in an adjacent land. 1 Damage in an adjacent land." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync(TargetSpiritCtx ctx ) {
		// Target Spirt may choose one of their Sacred Sites.
		Space space = await ctx.Other.Gateway.Decision(
			new ASpace( "Replace presence with badlands", ctx.Other.Presence.SacredSites, Present.Always )
		);

		await TargetSpiritActions( ctx.OtherCtx.Target( space ) );

		// if you have 3 moon,2 fire,2 air: 
		if(await ctx.YouHave("3 moon,2 fire,2 air" )){
			var sCtx = ctx.Target(space);
			// 1 damage in an adjactnt land.
			await DamageInAdjacentLand( sCtx );
			// 1 damage in an adjactnt land.
			await DamageInAdjacentLand( sCtx );
		}

	}

	static async Task DamageInAdjacentLand( TargetSpaceCtx ctx ) {
		var adjCtx = await ctx.SelectAdjacentLand("1 damage");
		if(adjCtx != null)
			await adjCtx.DamageInvaders(1);
	}


	static async Task TargetSpiritActions( TargetSpaceCtx otherCtx ) {

		// !!! needs to handle incarna too

		// In that land: Replace each of their presence with badlands;  The replaced presence leaves the game.
		int total = 0;

		foreach(var token in otherCtx.Self.Presence.TokensDeployedOn( otherCtx.Tokens ).ToArray()) {
			int count = otherCtx.Tokens[token];
			await otherCtx.Tokens.Remove(token, count );
			total += count;
		}
		await otherCtx.Badlands.AddAsync( total, AddReason.AsReplacement );

		// Push any number of those Badlands.
		await otherCtx.PushUpTo( total, Token.Badlands );

		// 3 fear.
		otherCtx.AddFear( 3 );

		// 3 damage per presence replaced
		await otherCtx.DamageInvaders( total * 3 );
	}

}