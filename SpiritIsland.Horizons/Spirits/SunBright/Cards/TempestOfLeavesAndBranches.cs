namespace SpiritIsland.Horizons;

public class TempestOfLeavesAndBranches {

	public const string Name = "Tempest of Leaves and Branches";

	[SpiritCard(Name, 2, Element.Sun, Element.Air, Element.Plant), Fast, FromSacredSite(1)]
	[Instructions("Choose up to 5 different Invaders.  1 Damage to each of them."), Artist(Artists.LucasDurham)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// Choose up to 5 different Invaders.
		var orig = new CountDictionary<HumanToken>( ctx.Space.HumanOfTag(TokenCategory.Invader).ToDictionary(x => x, x => ctx.Space[x]));
		for(int i=0; i < 5; ++i ) {
			var invader = await ctx.Self.Select($"Do 1 point of damage to invader ({i+1} of 5)", orig.Keys, Present.Done);
			if(invader is null) break;
			// 1 Damage to each of them.
			await ctx.Space.Invaders.ApplyDamageTo1(1,invader);
			orig[invader]--;
		}
	}

}
