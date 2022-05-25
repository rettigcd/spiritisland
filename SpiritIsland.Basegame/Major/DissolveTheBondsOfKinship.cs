namespace SpiritIsland.Basegame;

public class DissolveTheBondsOfKinship {
		
	[MajorCard("Dissolve the Bonds of Kinship",4,Element.Fire,Element.Earth,Element.Animal)]
	[Slow]
	[FromPresence(1)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// replace 1 city with 2 exploreres.
		await ReplaceInvader.SingleInvaderWithExplorers( ctx.Self, ctx.Invaders, Invader.City, 2 );

		// replace 1 town with 1 explorer
		await ReplaceInvader.SingleInvaderWithExplorers( ctx.Self, ctx.Invaders, Invader.Town, 1 );

		// replace 1 dahan with 1 explorer.
		if( await ctx.Dahan.Remove1(RemoveReason.Replaced) != null )
			await ctx.AddDefault( Invader.Explorer, 1, AddReason.AsReplacement );

		// if you have 2 fire 2 water 3 animal
		if(await ctx.YouHave("2 fire,2 water,3 animal" )) {
			// before pushing, explorers and city/town do damage to each other
			int damageFromExplorers = ctx.Tokens.Sum(Invader.Explorer);
			int damageToExplorers = ctx.Tokens.Sum(Invader.City)*3 + ctx.Tokens.Sum(Invader.Town)*2;
			await ctx.DamageInvaders(damageFromExplorers,Invader.City,Invader.Town);
			await ctx.DamageInvaders( damageToExplorers, Invader.Explorer );
		}

		// Push all explorers from target land to as many different lands as possible
		await ctx.Push( int.MaxValue,Invader.Explorer);
	}

}