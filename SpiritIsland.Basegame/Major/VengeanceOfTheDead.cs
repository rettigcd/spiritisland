namespace SpiritIsland.Basegame;

public class VengeanceOfTheDead {

	const string Name = "Vengeance of the Dead";

	[MajorCard(Name,3,Element.Moon,Element.Fire,Element.Animal), Fast, FromPresence(3)]
	[Instructions( "3 Fear. After each effect that destroys Town / City / Dahan in target land, 1 Damage per Town / City / Dahan destroyed. -If you have- 3 Animal: Damage from this Power may be dealt into adjacent lands." ), Artist( Artists.KatBirmelin )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 3 fear
		await ctx.AddFear(3);

		var landsWeCanApplyTheDamageTo = new List<SpaceSpec> { ctx.Space.SpaceSpec };

		// After each effect that destroys...
		ctx.Space.Adjust( new DealVengeanceDamageOnDestroy( ctx, landsWeCanApplyTheDamageTo ), 1 );

		// if you have 3 animal
		if(await ctx.YouHave( "3 animal" ))
			// damage may be dealt into adjacent lands
			landsWeCanApplyTheDamageTo.AddRange( ctx.Adjacent.Select( s => s.SpaceSpec ) );

	}

	public class DealVengeanceDamageOnDestroy( TargetSpaceCtx ctx, List<SpaceSpec> landsWeCanApplyTheDamageTo )
		: BaseModEntity, IEndWhenTimePasses, IHandleTokenRemoved
	{
		public async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
			if( !args.Reason.IsDestroy() ) return;
			//  ...a town / city / dahan in target land
			if( args.Removed.Class.IsOneOf( Human.Town_City.Plus( Human.Dahan ) ) )
				// 1 damage per token destroyed
				await DistributeDamageToLands( ctx, landsWeCanApplyTheDamageTo, 1 );
		}
	}

	static async Task DistributeDamageToLands( TargetSpaceCtx ctx, List<SpaceSpec> newDamageLandSpecs, int additionalDamage ) {
		var scope = ActionScope.Current;
		Space[] targetLandOptions  = newDamageLandSpecs.Select( spec => scope.AccessTokens( spec ) ).Where( s => s.HasInvaders() ).ToArray();
		var newLand = await ctx.Self.Select($"Apply up to {additionalDamage} vengeanance damage in:", targetLandOptions, Present.Always);
		if(newLand is not null)
			await ctx.Target( newLand ).DamageInvaders( 1 );
	}

}