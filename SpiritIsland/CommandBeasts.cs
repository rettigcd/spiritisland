using SpiritIsland.Log;

namespace SpiritIsland;

/// <summary>
/// Watches for the Invader card whose Code was recorded at Setup to reach the Build slot - meaning it just advanced off of Explore, i.e. it was revealed and explored last round.
/// 	=> once triggered, adds itself to the 1st Spirit's Mods bucket as an IModifyAvailableActions,
/// 	   offering itself as a Fast action each round until used, then removes itself from Mods.
/// </summary>
public class CommandBeasts( string title, string triggerCardCode = "" )
	: IActionFactory
	, IModifyAvailableActions
	, IRunAfterInvaderPhase
	, ILogEntry
	, ISerializableSpiritMod
	, ISerializableActionFactory
{

	#region ILogEntry
	LogLevel ILogEntry.Level => LogLevel.Info;
	string ILogEntry.Msg(LogLevel _) => $"Card Revealed: {Title} - {Desciption}";
#pragma warning disable CA1822 // Mark members as static
	public string Desciption => CommandBeastsOn1Space.InstructionText;
#pragma warning restore CA1822 // Mark members as static
	#endregion ILogEntry

	public const string Stage2 = "Command Beasts (II)";
	public const string Stage3 = "Command Beasts (III)";

	#region Static Setup

	static public void Setup( GameState gameState ) {
		// If there are no Event cards, compensate with Command-the-Beasts
		var cards = gameState.InvaderDeck.UnrevealedCards;

		// Stage 2
		int i = 0; while(cards[i].InvaderStage != 2) ++i;
		if(0<i && cards[i-1].InvaderStage==3) --i; // Brandenburg Prussia
		gameState.AddPostInvaderPhase( new CommandBeasts( Stage2, cards[i].Code ) );

		// Stage 3 - Find 1st card of last Level-3 group
		i = cards.Count;
		while(cards[i - 1].InvaderStage != 3) --i; // While prev is not level 3, backup - ends on card following level 3 group
		while(cards[i - 1].InvaderStage == 3) --i; // While prev is level 3, backup - ends on 1st level 3 card in last group
		gameState.AddPostInvaderPhase( new CommandBeasts( Stage3, cards[i].Code ) );
	}

	#endregion Static Setup

	string IOption.Text => Title;
	public string Title { get; } = title;

	public async Task ActivateAsync( Spirit spirit ) {
		spirit.Mods.Remove( this );
		GameState gs = GameState.Current;
		await using ActionScope actionScope = await ActionScope.Start(ActionCategory.Special);
		await new CommandBeastsOn1Space().In().EachActiveLand().ActAsync( gs );
		gs.ReminderCards.Remove(this);
	}

	public bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Fast;

	#region IRunAfterInvaderPhase

	/// <summary>
	/// Card just advanced off of Explore into Build => it was revealed and explored last round, so this
	/// is the round to swap in for the missing Event card.
	/// </summary>
	Task IRunAfterInvaderPhase.AfterInvaderPhase( GameState gameState ) {
		_triggered = gameState.InvaderDeck.Build.Cards.Any( c => c.Code == triggerCardCode );
		if(_triggered) {
			gameState.Log( this );
			gameState.ReminderCards.Add( this );
			gameState.Spirits[0].Mods.Add( this );
		}
		return Task.CompletedTask;
	}

	bool IRunAfterInvaderPhase.RemoveAfterRun => _triggered;
	bool _triggered;

	JsonArray IRunAfterInvaderPhase.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, Title, triggerCardCode );

	#endregion IRunAfterInvaderPhase

	#region IModifyAvailableActions

	/// <summary>
	/// Presence in Mods IS the "not yet used" signal - ActivateAsync removes this from Mods once used,
	/// so there's no separate used-flag to check here.
	/// </summary>
	void IModifyAvailableActions.Modify( List<IActionFactory> orig, Phase phase ) {
		if(phase == Phase.Fast)
			orig.Add( this );
	}

	#endregion IModifyAvailableActions

	#region Json

	/// <summary>
	/// [ Tag, Title ]. Covers this object's own Mods membership only - the gameState.ReminderCards entry
	/// (added alongside this once triggered) round-trips separately via GameState.ReminderCardsToJson/
	/// RestoreReminderCardsFromJson, which resolves back to this same restored instance by Title rather
	/// than constructing a disconnected duplicate.
	/// </summary>
	JsonArray ISerializableSpiritMod.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, Title );

	/// <summary>
	/// [ Tag, Title ]. Covers this instance once it's landed in Spirit._usedActions - ActivateAsync
	/// removes it from Mods (via IOwnedActionFactories-style reference resolution) before MarkAsResolved's
	/// caller even returns, so by the time a save happens post-use there's no live Mods entry left to
	/// resolve back through. A fresh instance is fine here (unlike the IOwnedActionFactories mods)
	/// because nothing compares this factory by reference once used - "not yet used" is Mods membership
	/// alone (see the IModifyAvailableActions remark below), not a UsedActions.Contains(sameInstance)
	/// check like PourDownPower/UnrelentingStrides/MarkedBeastMover rely on.
	/// </summary>
	JsonArray ISerializableActionFactory.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, Title );

	const string Tag = "CommandBeasts";

	[ModuleInitializer]
	internal static void RegisterSerialization() {
		SpiritModRegistry.Register( Tag, ( spirit, json, ctx ) => spirit.Mods.Add( new CommandBeasts( json[1]!.GetValue<string>() ) ) );
		PostInvaderPhaseActionRegistry.Register( Tag, ( json, ctx ) => new CommandBeasts( json[1]!.GetValue<string>(), json[2]!.GetValue<string>() ) );
		ActionFactoryRegistry.Register( Tag, ( json, ctx ) => new CommandBeasts( json[1]!.GetValue<string>() ) );
	}

	#endregion Json
}

/// <summary>
/// Commands-the-Beasts (non-events) for 1 space.
/// </summary>
public class CommandBeastsOn1Space : IActOn<TargetSpaceCtx> {

	public const string InstructionText = "For each (original) beast on land, push, do 1 damage, or 1 fear if invaders are present.";

	public string Description => InstructionText;

	public bool IsApplicable( TargetSpaceCtx ctx ) => OriginalBeastCounts[ctx.SpaceSpec] > 0;// use original, not current. Incase anything moved.

	public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Using this order: Damage / Push / Fear(based on starting invaders)
		//	Pro:	Assists user. Can use as much damage as desired (this is probably top priority) and not worry about using fear prior to killing explorers.
		//	Pro:	Doesn't require user input on fear count.  Can just apply whatever remains as fear.
		//	Con:	Action appear done out of order. (but this is ok since it is really simultaneous

		bool startedWithInvaders = ctx.HasInvaders;

		int count = OriginalBeastCounts[ctx.SpaceSpec];
		if(count == 0) return;

		// Damage
		count -= await PartialDamageToInvaders( ctx, count );

		// Push
		await ctx.SourceSelector
			.UseQuota(new Quota().AddGroup( count, Token.Beast ))
			.Track( _ => --count )
			.PushUpToN( ctx.Self );

		// Fear
		if(startedWithInvaders)
			await ctx.AddFear( count );

	}

	#region private

	CountDictionary<SpaceSpec> OriginalBeastCounts => _originalBeastCounts 
		??= ActionScope.Current.Spaces
				.ToDictionary(s => s.SpaceSpec, s => s.Beasts.Count)
				.ToCountDict();
	CountDictionary<SpaceSpec>? _originalBeastCounts;

	static async Task<int> PartialDamageToInvaders( TargetSpaceCtx ctx, int damage ) {
		if(damage == 0) return 0; // not necessary, just saves some cycles

		int badlandCount = ctx.Badlands.Count;

		CombinedDamage combinedDamage = ctx.Space.CombinedDamageFor_Invaders( damage );

		var quota = new DamageQuota_NoMods(combinedDamage.Available);
		await ctx.SourceSelector
			.UseQuota( quota )
			.DoDamageAsync( ctx.Self, combinedDamage.Available, Present.Done );
		int damageDone = quota.AppliedDamage;

		combinedDamage.TrackDamageDone( damageDone );

		return damageDone == 0
			? 0 // no damage done
			: Math.Max( 1, damageDone - badlandCount ); // some damage done, remove badland damage.

	}

	#endregion
}


/*
Don’t use a Blight Card where:
	- the Blighted Island side has 2 Blight per player or 
	- only immediate effects. 

You can either pull those cards out before shuffling, or redraw if you get one.
Without an Event Deck to provide occasional Blighted island Events, cards with beneficial
or non-ongoing effects become much lower-risk. 

Still-Healthy Island Cards are fine, though staying Healthy has less game impact.

Cards with 2 Blight/player: 
	Aid from Lessor Sprits, 
	Back Against the Wall

Cars with Immediate effects only:
	Tipping point
	Erosion of Will
	Distinigrating Ecosystem
	A Pall upon the land
	Untended land crumbles
	Unnatural Proliferation
	Thriving Communitites
	Promising Farmlands

Leaving Only:
	All Things Weaken
	Downward Spiral
	Memory Fades to Dust
	Power Corrodes the Spirit

*/