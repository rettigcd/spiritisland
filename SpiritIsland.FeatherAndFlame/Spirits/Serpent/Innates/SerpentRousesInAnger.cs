namespace SpiritIsland.FeatherAndFlame;

[InnatePower( Name ), Slow, FromPresence(0)]
public class SerpentRousesInAnger {

	public const string Name = "Serpent Rouses in Anger";

	[InnateTier( "1 fire,1 earth","For each fire earth you have, 1 Damage to 1 town / city." )]
	static public async Task Option1Async( TargetSpaceCtx ctx ) {
		// For each fire & earth you have
		var els = ctx.Self.Elements;
		int count = Math.Min( await els.GetAsync(Element.Fire), await els.GetAsync(Element.Earth));
		// 1 Damage to 1 town / city.
		await ctx.Tokens.UserSelected_DamageInvadersAsync( ctx.Self, count, Human.Town_City );
	}

	[InnateTier( "2 moon 2 earth", "For each 2 moon 2 earth you have, 2 fear and you may Push 1 town from target land." )]
	static public async Task Option2Async( TargetSpaceCtx ctx ) {
		await Option1Async( ctx );

		// For each 2 moon 2 earth you have
		var els = ctx.Self.Elements;
		int count = Math.Min( await els.GetAsync(Element.Moon), await els.GetAsync(Element.Earth)) / 2;
		// 2 fear
		ctx.AddFear( count*2 );
		// you may Push 1 town from target land.
		await ctx.PushUpTo( count, Human.Town );
	}

	[InnateTier("5 moon,6 fire,6 earth", "-7 Energy.  In every land in the game: X Damage, where X is the number of presence you have in and adjacent to that land." )]
	static public async Task Option3Async( TargetSpaceCtx ctx ) {
		await Option2Async( ctx );

		if(7 <= ctx.Self.Energy && await ctx.Self.UserSelectsFirstText("Activate Teir 3?","Pay 7 to cause damage from presence", "skip" )){
			// -7 Energy.
			ctx.Self.Energy -= 7;
			// In every land in the game: X Damage, where X is the number of presence you have in and adjacent to that land.
			var invaderLands = GameState.Current.Spaces
				.Where(space => space.HasInvaders())
				.ToArray();
			foreach(var land in invaderLands) {
				var landsCreatingDamage = new HashSet<Space>(land.InOrAdjacentTo.Downgrade());
				int damage = land.InOrAdjacentTo.Sum( ctx.Self.Presence.CountOn );
				await ctx.DamageInvaders(damage);
			}
		}

	}

}