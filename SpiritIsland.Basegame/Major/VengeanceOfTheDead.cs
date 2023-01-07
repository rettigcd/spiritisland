namespace SpiritIsland.Basegame;

public class VengeanceOfTheDead {

	const string Name = "Vengeance of the Dead";

	[MajorCard(Name,3,Element.Moon,Element.Fire,Element.Animal), Fast, FromPresence(3)]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 3 fear
		ctx.AddFear(3);

		var landsWeCanApplyTheDamageTo = new List<SpaceState> { ctx.Tokens };

		// After each effect that destroys...
		async Task DealVengenceDamage( ITokenRemovedArgs args ) {
			if( !args.Reason.IsDestroy() ) return;
			//  ...a town / city / dahan in target land
			if( args.Token.Class.IsOneOf( Invader.Town_City.Plus( TokenType.Dahan ) ) )
				// 1 damage per token destroyed
				await DistributeDamageToLands( ctx, landsWeCanApplyTheDamageTo, 1 );
		}
		ctx.Tokens.Adjust( new TokenRemovedHandler(Name, DealVengenceDamage ), 1 );

		// if you have 3 animal
		if(await ctx.YouHave( "3 animal" ))
			// damage may be dealt into adjacent lands
			landsWeCanApplyTheDamageTo.AddRange( ctx.Adjacent );

	}

	static async Task DistributeDamageToLands( TargetSpaceCtx ctx, List<SpaceState> newDamageLands, int additionalDamage ) {
		Space[] targetLandOptions  = newDamageLands.Where( s => s.HasInvaders() ).Select( x => x.Space ).ToArray();
		var newLand = await ctx.Decision( new Select.Space( $"Apply up to {additionalDamage} vengeanance damage in:", targetLandOptions, Present.Always ));
		if(newLand != null)
			await ctx.Target( newLand ).DamageInvaders( 1 );
	}

}