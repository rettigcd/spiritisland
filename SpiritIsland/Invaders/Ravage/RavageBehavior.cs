using SpiritIsland.Invaders.Ravage;

namespace SpiritIsland;

/// <summary> Configures Dahan and Invader behavior on a per-space bases. </summary>
public sealed class RavageBehavior : ISpaceEntity, IEndWhenTimePasses, ISerializableSpaceEntity {

	public static RavageBehavior DefaultBehavior => s_defaultRavageBehavior;

	/// <summary> Registered by mods that override the Order / Who is damaged. Last-registered runs outermost, same as the old delegate-wrapping order. </summary>
	public List<IRavageSequenceStep> SequenceSteps = [];

	/// <summary> Registered by mods that adjust attacker damage (e.g. neighboring towns, blight cards). Applied in registration order. </summary>
	public List<IAdjustAttackerDamage> DamageAdjusters = [];

	public int AttackersDefend = 0; // reduces the damage inflicted by the defenders onto the attackers.  Not exactly correct, but close

	public int CalculateDamageFromParticipatingAttackers( RavageExchange rex ) {
		int total = GetDamageFromParticipatingAttackers_Default( rex );
		foreach( IAdjustAttackerDamage adjuster in DamageAdjusters )
			total = adjuster.Adjust( rex, total );
		return total;
	}

	async Task RunSequence( RavageData data ) {
		Func<Task> next = () => RavageSequence_Default( this, data );
		foreach( IRavageSequenceStep step in SequenceSteps ) {
			var current = next; // capture, so each iteration wraps its own inner chain
			next = () => step.Execute( this, data, current );
		}
		await next();
	}

	public RavageBehavior Clone() {
		return new RavageBehavior {
			SequenceSteps   = [.. SequenceSteps],
			DamageAdjusters = [.. DamageAdjusters],
			AttackersDefend = AttackersDefend
		};
	}

	#region Serialization

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray(
		Tag,
		new JsonArray( SequenceSteps.Select( s => (JsonNode)SerializeMember( s, ctx ) ).ToArray() ),
		new JsonArray( DamageAdjusters.Select( a => (JsonNode)SerializeMember( a, ctx ) ).ToArray() ),
		AttackersDefend
	);

	static JsonArray SerializeMember( object member, ISerializationContext ctx ) => member is ISerializableSpaceEntity entity
		? entity.ToJson( ctx )
		: throw new NotSupportedException( $"{member.GetType()} must implement ISerializableSpaceEntity to be added to RavageBehavior's SequenceSteps/DamageAdjusters" );

	const string Tag = "RavageBehavior";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new RavageBehavior {
			SequenceSteps = [.. json[1]!.AsArray().Select( n => (IRavageSequenceStep)SpaceEntitySerialization.Deserialize( (JsonArray)n!, ctx ) )],
			DamageAdjusters = [.. json[2]!.AsArray().Select( n => (IAdjustAttackerDamage)SpaceEntitySerialization.Deserialize( (JsonArray)n!, ctx ) )],
			AttackersDefend = json[3]!.GetValue<int>()
		} );

	#endregion

	/// <summary> Executes up to 1 potential Ravage </summary>
	public async Task Exec( Space space ) {

		RavageData data = new RavageData( space );

		var scope = await ActionScope.Start( ActionCategory.Invader ); // start scope before Stoppers run

		// Check for Stoppers
		var stoppers = data.Space.ModsOfType<ISkipRavages>()
			.OrderBy( s => s.Cost )
			.ToArray();

		foreach(ISkipRavages stopper in stoppers)
			if(await stopper.Skip( data.Space )) {
				// baby steps, don't break tests.  Eventually we want: $"stopped by {stopper.SourceLabel}";
				return; 
			}

		// Config the ravage
		foreach(IConfigRavages configurer in data.Space.ModsOfType<IConfigRavages>().ToArray() )
			await configurer.Config( data.Space );

		try {
			await RunSequence( data );

			ActionScope.Current.Log( new Log.RavageEntry( [..data.Result] ) );
		}
		finally {
			if(scope != null) {
				await scope.DisposeAsync();
			}
		}

	}

	static async Task RavageSequence_Default( RavageBehavior behavior, RavageData data ) {

		var ravageRounds = new RavageOrder[] { RavageOrder.Ambush, RavageOrder.InvaderTurn, RavageOrder.DahanTurn };
		foreach(RavageOrder ravageRound in ravageRounds) {
			var exchange = new RavageExchange( data.Space, ravageRound );
			if(exchange.HasActiveParticipants) {
				await exchange.Execute( behavior );
				data.Result.Add( exchange );
			}
		}
		await DamageLand( data );
	}

	static int GetDamageFromParticipatingAttackers_Default( RavageExchange rex ) {
		Space space = rex.Space;
		return rex.Attackers.Active.Keys
			.OfType<HumanToken>()
			.Where( x => x.StrifeCount == 0 )
			.Select( attacker => attacker.Attack * rex.Attackers.Starting[attacker] ).Sum();
	}

	static public async Task DamageLand( RavageData ra ) {
		int totalLandDamage = ra.Result.Sum( r => r.Attackers.DamageDealtOut );
		await ra.InvaderBinding.Space.DamageLand( totalLandDamage ); // must call even if there is 0 Damage incase a modifier adds some.
	}

	// This is never modified. It is cloned and the clone is modified.
	static public RavageBehavior GetDefault() => s_defaultRavageBehavior.Clone();
	static readonly RavageBehavior s_defaultRavageBehavior = new RavageBehavior();

}

/// <summary> Registers with RavageBehavior.DamageAdjusters to adjust attacker damage during a Ravage. </summary>
public interface IAdjustAttackerDamage {
	int Adjust( RavageExchange rex, int runningTotal );
}

/// <summary> Registers with RavageBehavior.SequenceSteps to override how a Ravage plays out. Call next() to defer to whatever would have run otherwise. </summary>
public interface IRavageSequenceStep {
	Task Execute( RavageBehavior behavior, RavageData data, Func<Task> next );
}