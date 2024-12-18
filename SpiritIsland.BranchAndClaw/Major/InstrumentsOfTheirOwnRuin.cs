namespace SpiritIsland.BranchAndClaw;

public class InstrumentsOfTheirOwnRuin {

	[MajorCard( "Instruments of Their Own Ruin", 4, Element.Sun, Element.Fire, Element.Air, Element.Animal ),Fast,FromSacredSite( 1 )]
	[Instructions( "Add 1 Strife. Each Invader with Strife deals Damage to other Invaders in target land. -If you have- 4 Sun, 2 Fire, 2 Animal: Instead, if Invaders Ravage in target land, they damage Invaders in adjacent lands instead of Dahan and the land. Dahan in target land do not fight back." ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		await ctx.SelectActionOption( 
			new SpaceAction(
				"Add strife. Invaders with strife deal Damage to other Invaders in target land.", 
				AddStrifeThenStrifedInvadersDamageUnstrifed
			)
			, new SpaceAction(
				"Instead, if Invaders Ravage in target land, damage invaders in adjacent lands instead of dahan"
				, DuringRavage_InvadersDamageInvadersInAdjacentLandsInsteadOfDahan
			).OnlyExecuteIf( await ctx.YouHave("4 sun,2 fire,2 animal" ) )
		);

	}

	public static async Task AddStrifeThenStrifedInvadersDamageUnstrifed( TargetSpaceCtx ctx ) {
		// add 1 strife
		await ctx.AddStrife();
		// strifed invaders deal damage to other invaders in target land.
		await StrifedRavage.StrifedInvadersDealsDamageToOtherInvaders.ActAsync( ctx );
	}

	static Task DuringRavage_InvadersDamageInvadersInAdjacentLandsInsteadOfDahan( TargetSpaceCtx ctx ) {
		ctx.ModifyRavage( cfg => cfg.RavageSequence = ( _, _ ) => RavageSequence_DamageInvadersInAdjacentLand(ctx ) );
		return Task.CompletedTask;
	}

	static async Task RavageSequence_DamageInvadersInAdjacentLand( TargetSpaceCtx ctx ) { 

		// Note - this works regardless of them ravaging in target land or not. yay!

		// This ravage is totally different from any other.
		// - occurs in multiple spaces
		// - total damage is from a single centralized space
		// - When applying damage, prefer to use up badland damage first, and save general distributed damage for other spaces.

		// - This isn't perfect but it is good enough.

		// they damage invaders in adjacent lands instead of dahan and the land.

		// Get damage
		int damageFromCenter = ctx.Space.InvaderTokens().OfType<HumanToken>()
			.Where( x => x.StrifeCount > 0 )
			.Sum( si => si.Attack * ctx.Space[si] );

		// Calc Total Badlands damage available
		CountDictionary<Space> availableBadlandDamage = ctx.Adjacent.ToDictionary(x=>x,x=>x.Badlands.Count).ToCountDict(); // captures # of badlands then sets to 0 once space is activated.
		CountDictionary<Space> activatedBadlandDamage = []; // initializes when they do first damage in land, then used until depleated
		bool HasDamage(Space space) => 0 < damageFromCenter || activatedBadlandDamage[space]>0;

		// While any invaders && (damageFromStrifed>0 || )damage && any explorers
		Space[] spaceOptions;
		while( (spaceOptions = ctx.Adjacent.Where( adj => adj.HasInvaders() && HasDamage( adj ) ).ToArray()).Length > 0 ) {
			// select target invader
			var invaderOptions = spaceOptions
				.SelectMany( ss => ss.InvaderTokens().On(ss) )
				.ToArray();
			var damagedInvader = await ctx.Self.SelectAsync( new A.SpaceTokenDecision($"Instrument of Ruin Damage ({damageFromCenter}) remaining", invaderOptions,Present.Done) );
			if(damagedInvader is null) break;

			if(0 < activatedBadlandDamage[damagedInvader.Space]) {
				// use badlands
				activatedBadlandDamage[damagedInvader.Space]--;
			} else {
				// use original
				damageFromCenter--;
				// if badlands not activated
				activatedBadlandDamage[damagedInvader.Space] = availableBadlandDamage[damagedInvader.Space];
				availableBadlandDamage[damagedInvader.Space] = 0;
			}

			// apply 1 damage to selected invader
			// !Note - using shared UnitOfWork across spaces because it is a ravage on only 1 space
			await damagedInvader.Space.Invaders
				.ApplyDamageTo1( 1, damagedInvader.Token.AsHuman() );
		}

	}

}