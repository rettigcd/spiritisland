﻿namespace SpiritIsland.Basegame;

public class DissolveTheBondsOfKinship {
		
	[MajorCard("Dissolve the Bonds of Kinship",4,Element.Fire,Element.Earth,Element.Animal)]
	[Slow]
	[FromPresence(1)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// replace 1 city with 2 exploreres.
		await ReplaceInvader.SingleInvaderWithExplorers( ctx, Invader.City, 2 );

		// replace 1 town with 1 explorer
		await ReplaceInvader.SingleInvaderWithExplorers( ctx, Invader.Town, 1 );

		// replace 1 dahan with 1 explorer.
		if( await ctx.Dahan.Remove1(RemoveReason.Replaced) != null )
			await ctx.AddDefault( Invader.Explorer, 1, AddReason.AsReplacement );

		// if you have 2 fire 2 water 3 animal
		if(await ctx.YouHave("2 fire,2 water,3 animal" )) {
			// before pushing, explorers and city/town do damage to each other
			int damageFromExplorers = ctx.Tokens.Sum(Invader.Explorer);
			int damageToExplorers = ctx.Tokens.Sum(Invader.City) * Invader.City.Attack
				+ ctx.Tokens.Sum(Invader.Town) * Invader.Town.Attack;
			await ctx.DamageInvaders(damageFromExplorers,Invader.City,Invader.Town);
			await ctx.DamageInvaders( damageToExplorers, Invader.Explorer );
		}

		// Push all explorers from target land to as many different lands as possible
		await ctx.Push( int.MaxValue,Invader.Explorer);

		var unpushedLands = ctx.Adjacent.ToList();
		int explorerCount = ctx.Tokens.Sum( Invader.Explorer );
		while(explorerCount > 0) {
			// select token
			var tokenOptions = ctx.Tokens.OfType( Invader.Explorer ).ToArray();
			var decision = Select.TokenFrom1Space.TokenToPush( ctx.Space, explorerCount, tokenOptions, Present.Always );
			var token = await ctx.Self.Action.Decision( decision );
			if(token == null) break;

			// Select destination
			var destinationOptions = explorerCount > unpushedLands.Count
				? ctx.Adjacent.ToArray() // allow anywhere
				: unpushedLands.ToArray(); // force to un-pushed lands
			var destination = await ctx.Decision( Select.Space.PushToken( token, ctx.Space, destinationOptions, Present.Always ) );

			// Move
			await ctx.Move(token,ctx.Space,destination);

			// next
			unpushedLands.Remove(destination);
			--explorerCount;
		}

	}

}