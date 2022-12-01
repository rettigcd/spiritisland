namespace SpiritIsland.BranchAndClaw;

public class InstrumentsOfTheirOwnRuin {

	[MajorCard( "Instruments of Their Own Ruin", 4, Element.Sun, Element.Fire, Element.Air, Element.Animal )]
	[Fast]
	[FromSacredSite( 1 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		await ctx.SelectActionOption( 
			new SpaceAction(
				"Add strife. Invaders with strife deal Damage to other Invaders in target land.", 
				AddStrifeThenStrifedInvadersDamageUnstrifed
			)
			, new SpaceAction(
				"Instead, if Invaders Ravage in target land, damage invaders in adjacent lands instead of dahan"
				, DuringRavage_InvadersDamageInvadersInAdjacentLandsInsteadOfDahan
			).FilterOption( await ctx.YouHave("4 sun,2 fire,2 animal" ) )
		);

	}

	public static async Task AddStrifeThenStrifedInvadersDamageUnstrifed( TargetSpaceCtx ctx ) {
		// add 1 strife
		await ctx.AddStrife();
		// strifed invaders deal damage to other invaders in target land.
		await StrifedRavage.StrifedInvadersDealsDamageToOtherInvaders.Execute( ctx );
	}


	static Task DuringRavage_InvadersDamageInvadersInAdjacentLandsInsteadOfDahan( TargetSpaceCtx ctx ) {
		ctx.ModifyRavage( cfg => cfg.RavageSequence = eng => RavageSequence_DamageInvadersInAdjacentLand(ctx, ctx.GameState.StartAction() ) );
		return Task.CompletedTask;
	}

	static async Task RavageSequence_DamageInvadersInAdjacentLand( TargetSpaceCtx ctx, UnitOfWork actionId ) {
		// Note - this works regardless of them ravaging in target land or not. yay!

		// This ravage is totally different from any other.
		// - occurs in multiple spaces
		// - total damage is from a single centralized space
		// - When applying damage, prefer to use up badland damage first, and save general distributed damage for other spaces.
		// - We can't use BonusDamage Pool because that doesn't have a short-circuit to not use all of the bonus damage.

		// they damage invaders in adjacent lands instead of dahan and the land.

		// Get damage
		int damageFromCenter = ctx.Tokens.InvaderTokens().OfType<HealthToken>()
			.Where( x => x.StrifeCount > 0 )
			.Sum( si => ctx.Tokens.AttackDamageFrom1( si ) * ctx.Tokens[si] );

		// Calc Total Badlands damage available
		var tokens = ctx.GameState.Tokens;
		var availableBadlandDamage = ctx.Adjacent.ToDictionary(x=>x.Space,x=>x.Badlands.Count).ToCountDict(); // captures # of badlands then sets to 0 once space is activated.
		var activatedBadlandDamage = new CountDictionary<Space>(); // initializes when they do first damage in land, then used until depleated
		bool HasDamage(Space space) => damageFromCenter > 0 || activatedBadlandDamage[space]>0;

		// While any invaders && (damageFromStrifed>0 || )damage && any explorers
		SpaceState[] spaceOptions;
		while( (spaceOptions = ctx.Adjacent.Where( adj => adj.HasInvaders() && HasDamage( adj.Space ) ).ToArray()).Length > 0 ) {
			// select target invader
			var invaderOptions = spaceOptions.SelectMany(space=>space.InvaderTokens().Select(t=>new SpaceToken(space.Space,t))).ToArray();
			var invader = await ctx.Decision( new Select.TokenFromManySpaces($"Instrument of Ruin Damage ({damageFromCenter}) remaining", invaderOptions,Present.Done) );
			if(invader == null) break;

			if(activatedBadlandDamage[invader.Space] > 0) {
				// use badlands
				activatedBadlandDamage[invader.Space]--;
			} else {
				// use original
				damageFromCenter--;
				// if badlands not activated
				activatedBadlandDamage[invader.Space] = availableBadlandDamage[invader.Space];
				availableBadlandDamage[invader.Space] = 0;
			}

			// apply 1 damage to selected invader
			await ctx.GameState.Invaders.On(invader.Space, actionId ).ApplyDamageTo1(1,(HealthToken)invader.Token);
		}

	}

	// Deal

}