namespace SpiritIsland.Basegame;

public class DissolveTheBondsOfKinship {
	public const string Name = "Dissolve the Bonds of Kinship";

	[MajorCard(Name,4,Element.Fire,Element.Earth,Element.Animal),Slow,FromPresence(1)]
	[Instructions( "Replace 1 City with 2 Explorer. Replace 1 Town with 1 Explorer. Replace 1 Dahan with 1 Explorer. Push all Explorer from target land to as many different lands as possible. -If you have- 2 Fire, 2 Water, 3 Animal: Before Pushing, Explorer and Town / City do Damage to each other." ), Artist( Artists.JorgeRamos )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// replace 1 city with 2 exploreres.
		await ReplaceInvader.DisolveInvaderIntoExplorers( ctx, Human.City, 2 );

		// replace 1 town with 1 explorer
		await ReplaceInvader.DisolveInvaderIntoExplorers( ctx, Human.Town, 1 );

		// replace 1 dahan with 1 explorer.
		await ReplaceDahanWithExplorer( ctx );

		// if you have 2 fire 2 water 3 animal
		if(await ctx.YouHave( "2 fire,2 water,3 animal" ))
			// before pushing, explorers and city/town do damage to each other
			await ExplorersAndCityTownsDamageEachOther( ctx );

		// Push all explorers from target land to as many different lands as possible
		await ctx.Pusher
			.AddGroup( int.MaxValue, Human.Explorer )
			.Config( Distribute.ToAsManyLandsAsPossible )
			.DoN();

		static int GetAttackDamageFrom( TargetSpaceCtx ctx, HumanTokenClass cc ) {
			return ctx.Tokens.HumanOfTag( cc ).Sum( t => ctx.Tokens[t] * t.Attack );
		}

		static async Task ExplorersAndCityTownsDamageEachOther( TargetSpaceCtx ctx ) {
			int damageFromExplorers = GetAttackDamageFrom( ctx, Human.Explorer );
			int damageToExplorers = GetAttackDamageFrom( ctx, Human.City ) + GetAttackDamageFrom( ctx, Human.Town );
			await ctx.DamageInvaders( damageFromExplorers, Human.Town_City );
			await ctx.DamageInvaders( damageToExplorers, Human.Explorer );
		}
	}

	static async Task ReplaceDahanWithExplorer( TargetSpaceCtx ctx ) {
		var toRemove = (await ctx.Tokens.RemovableOfAnyClass( RemoveReason.Replaced, Human.Dahan ))
			.Cast<HumanToken>()
			.OrderBy( x => x.RemainingHealth )
			.FirstOrDefault();
		if(toRemove is not null && (await ctx.Dahan.Remove1( toRemove, RemoveReason.Replaced )).Count == 1)
			await ctx.AddDefault( Human.Explorer, 1, AddReason.AsReplacement );
	}
}