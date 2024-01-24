namespace SpiritIsland;

/// <summary>
/// Configures Dahan and Invader behavior on a per-space bases.
/// </summary>
public sealed class RavageBehavior : ISpaceEntity, IEndWhenTimePasses {

	public static RavageBehavior DefaultBehavior => RavageBehavior._defaultRavageBehavior;

	// Order / Who is damaged
	public Func<RavageBehavior, RavageData, Task> RavageSequence = RavageSequence_Default;

	// Gets the Aggregate damage from attackers. (default action does this by calling AttackDamageFrom1)
	// !! This can be removed if we slap a Town-Tracking token on every space Which updates a 'NeighboringTownsToken' that does damage.
	public Func<RavageExchange,int> GetDamageFromParticipatingAttackers = GetDamageFromParticipatingAttackers_Default;

	public int AttackersDefend = 0; // reduces the damage inflicted by the defenders onto the attackers.  Not exactly correct, but close

	public RavageBehavior Clone() {
		return new RavageBehavior {
			RavageSequence                      = RavageSequence,
			GetDamageFromParticipatingAttackers = GetDamageFromParticipatingAttackers,
			AttackersDefend                     = AttackersDefend
		};
	}

	/// <summary> Executes up to 1 potential Ravage </summary>
	public async Task Exec( SpaceState tokens ) {
		RavageData data = new RavageData( tokens );

		var scope = await ActionScope.Start( ActionCategory.Invader ); // start scope before Stoppers run

		// Check for Stoppers
		var stoppers = data.Tokens.ModsOfType<ISkipRavages>()
			.OrderBy( s => s.Cost )
			.ToArray();

		foreach(ISkipRavages stopper in stoppers)
			if(await stopper.Skip( data.Tokens )) {
				// baby steps, don't break tests.  Eventually we want: $"stopped by {stopper.SourceLabel}";
				return; 
			}

		// Config the ravage
		foreach(IConfigRavagesAsync configurer in data.Tokens.ModsOfType<IConfigRavagesAsync>().ToArray() )
			await configurer.ConfigAsync( data.Tokens );
		foreach(IConfigRavages configurer in data.Tokens.ModsOfType<IConfigRavages>().ToArray() )
			configurer.Config( data.Tokens );

		data.InvaderBinding = data.Tokens.Invaders;

		try {
			await RavageSequence( this, data );

			ActionScope.Current.Log( new Log.RavageEntry( [..data.Result] ) );
		}
		finally {
			if(scope != null) {
				await scope.DisposeAsync();
				data.InvaderBinding = null;
			}
		}
	}

	static async Task RavageSequence_Default( RavageBehavior behavior, RavageData data ) {

		var ravageRounds = new RavageOrder[] { RavageOrder.Ambush, RavageOrder.InvaderTurn, RavageOrder.DahanTurn };
		foreach(RavageOrder ravageRound in ravageRounds) {
			var exchange = new RavageExchange( data.Tokens, ravageRound );
			if(exchange.HasActiveParticipants) {
				await exchange.Execute( behavior );
				data.Result.Add( exchange );
			}
		}
		await DamageLand( data );
	}

	static int GetDamageFromParticipatingAttackers_Default( RavageExchange rex ) {
		SpaceState tokens = rex.Tokens;
		return rex.Attackers.Active.Keys
			.OfType<HumanToken>()
			.Where( x => x.StrifeCount == 0 )
			.Select( attacker => attacker.Attack * rex.Attackers.Starting[attacker] ).Sum();
	}

	static public async Task DamageLand( RavageData ra ) {
		int totalLandDamage = ra.Result.Sum( r => r.Attackers.DamageDealtOut );
		await ra.InvaderBinding.Tokens.DamageLand( totalLandDamage ); // must call even if there is 0 Damage incase a modifier adds some.
	}

	// This is never modified. It is cloned and the clone is modified.
	static public RavageBehavior GetDefault() => _defaultRavageBehavior.Clone();
	static readonly RavageBehavior _defaultRavageBehavior = new RavageBehavior();

}