namespace SpiritIsland.FeatherAndFlame;

[InnatePower( "Firestorm" ),Fast]
[FromPresence(0,Target.Blight)]
public class FireStorm {

	// Group 0
	[InnateTier( "1 plant", "1 Damage per 2 fire you have.", 0 )]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		int fireDamage = await ctx.Self.Elements.GetAsync(Element.Fire) / 2; // rounding down
		await DoFireDamageAsync( ctx, fireDamage );
	}

	[InnateTier( "3 plant", "Instead, 1 Damage per fire you have.", 0 )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		int fireDamage = await ctx.Self.Elements.GetAsync(Element.Fire);
		await DoFireDamageAsync( ctx, fireDamage );
	}

	static async Task DoFireDamageAsync( TargetSpaceCtx ctx, int fireDamage ) {
		if( fireDamage == 0) return;
		if( await CanSplitDamage(ctx) ) 
			await DoFireDamageToMultipleTargets( ctx, fireDamage );
		else
			await ctx.DamageInvaders( fireDamage );
	}

	private static async Task DoFireDamageToMultipleTargets( TargetSpaceCtx ctx, int fireDamage ) {
		await ctx.DamageInvaders( 1 ); // because they targetted a land, only select invaders from that land.
		--fireDamage;

		var spacesWithPresenceAndBlight = ctx.Self.Presence.Spaces.Tokens()
			.Where( x=>x.Blight.Any )
			.ToArray();

		// ! don't .ToArray() this because we want it to re-execute each time.
		var invaderTokens = spacesWithPresenceAndBlight
			.SelectMany( ss => ss.InvaderTokens().On(ss.Space) );

		while(fireDamage > 0 && invaderTokens.Any()) {
			SpaceToken token = await ctx.SelectAsync( new A.SpaceToken($"Apply fire damage. ({fireDamage} remaining)",invaderTokens,Present.Always));
			await ctx.Target(token.Space).Invaders.ApplyDamageTo1(1,token.Token.AsHuman());
			--fireDamage;
		}
	}


	[DisplayOnly( MultiTargetThreshold, "You may split this Power's damage among any number of lands with blight where you have presence." )]
	static Task<bool> CanSplitDamage(TargetSpaceCtx ctx) => ctx.YouHave( MultiTargetThreshold );
	const string MultiTargetThreshold = "4 fire,2 air";

	// Group 1
	[InnateTier( "7 fire", "In a land with blight where you have presence, Push all dahan.  Destroy all Invaders and beast. 1 blight.", 1 )]
	static public async Task Option4( TargetSpaceCtx ctx ) {
		// In a land with blight and presence  (Select a space, not necessarily the one you targetted with power (I guess...)
		var spacesWithPresenceAndBlight = ctx.Self.Presence.Spaces.Tokens().Where( s=>s.Blight.Any );
		var space = await ctx.SelectAsync( new A.Space($"Push all dahan, destroy invaders and beast, 1 blight",spacesWithPresenceAndBlight,Present.Always));
		var spaceCtx = ctx.Target( space );

		// Push all Dahan
		await spaceCtx.PushDahan( int.MaxValue );

		// Destroy all invaders and Beasts
		var beasts = spaceCtx.Beasts;
		await beasts.Destroy( beasts.Count );
		await spaceCtx.Invaders.DestroyNOfAnyClass(int.MaxValue,Human.Invader);

		// Add 1 blight
		await ctx.AddBlight(1);
			
	}

}