namespace SpiritIsland.Basegame;

public class DissolveTheBondsOfKinship {
	public const string Name = "Dissolve the Bonds of Kinship";

	[MajorCard(Name,4,Element.Fire,Element.Earth,Element.Animal)]
	[Slow]
	[FromPresence(1)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// replace 1 city with 2 exploreres.
		await ReplaceInvader.SingleInvaderWithExplorers( ctx, Human.City, 2 );

		// replace 1 town with 1 explorer
		await ReplaceInvader.SingleInvaderWithExplorers( ctx, Human.Town, 1 );

		// replace 1 dahan with 1 explorer.
		var toRemove = (await ctx.Tokens.RemovableOfAnyClass(RemoveReason.Replaced, Human.Dahan))
			.Cast<HumanToken>()
			.OrderBy( x => x.RemainingHealth ).FirstOrDefault();
		if( await ctx.Dahan.Remove1( toRemove, RemoveReason.Replaced) != null )
			await ctx.AddDefault( Human.Explorer, 1, AddReason.AsReplacement );

		// if you have 2 fire 2 water 3 animal
		if(await ctx.YouHave("2 fire,2 water,3 animal" )) {
			// before pushing, explorers and city/town do damage to each other
			int damageFromExplorers = GetAttackDamageFrom( ctx, Human.Explorer );
			int damageToExplorers = GetAttackDamageFrom( ctx, Human.City ) + GetAttackDamageFrom( ctx, Human.Town );
			await ctx.DamageInvaders( damageFromExplorers, Human.Town_City );
			await ctx.DamageInvaders( damageToExplorers, Human.Explorer );
		}

		// Push all explorers from target land to as many different lands as possible
		await ctx.Push( int.MaxValue,Human.Explorer);

		var unpushedLands = ctx.Tokens.Adjacent.ToList();
		int explorerCount = ctx.Tokens.Sum( Human.Explorer );
		while(explorerCount > 0) {
			// select token
			var tokenOptions = ctx.Tokens.OfHumanClass( Human.Explorer ).ToArray();

			var token = (await ctx.Self.Gateway.Decision( Select.TokenFrom1Space.TokenToPush( ctx.Space, explorerCount, tokenOptions, Present.Always ) ))?.Token;
			if(token == null) break;

			// Select destination
			var destinationOptions = explorerCount > unpushedLands.Count
				? ctx.Tokens.Adjacent.ToArray() // allow anywhere
				: unpushedLands.ToArray(); // force to un-pushed lands
			var destination = await ctx.Decision( Select.ASpace.PushToken( token, ctx.Space, destinationOptions, Present.Always ) );
			var dstTokens = destination.Tokens;

			// Move
			await ctx.Move(token,ctx.Space,destination);

			// next
			unpushedLands.Remove( dstTokens );
			--explorerCount;
		}

		static int GetAttackDamageFrom( TargetSpaceCtx ctx, HumanTokenClass cc ) {
			return ctx.Tokens.OfHumanClass( cc ).Sum( t => ctx.Tokens[t] * ctx.Tokens.AttackDamageFrom1( t ) );
		}
	}

}