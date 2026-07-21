namespace SpiritIsland;

public abstract partial class Spirit 
	: IOption
	, IHaveASpirit // so all 'contexts' can use the same Picker and have a spirit to do the decision making.
	, IRunWhenTimePasses
	, IHaveMemento 
{

	string IOption.Text => SpiritName;

	public abstract string SpiritName { get; }

	public SpecialRule[] SpecialRules { get; set; } = [];

	public InnatePower[] InnatePowers { get; set; } = [];

	#region constructor

	public Spirit( Func<Spirit, SpiritPresence> initPresence, GrowthTrack growthTrack, params PowerCard[] initialCards ) {

		Presence = initPresence( this );
		GrowthTrack = growthTrack;

		foreach(var card in initialCards)
			AddCardToHand( card );

		_gateway = new UserGateway();

		decks.Add( new SpiritDeck { Type = SpiritDeck.DeckType.Hand, Cards = Hand } );
		decks.Add( new SpiritDeck { Type = SpiritDeck.DeckType.InPlay, Cards = InPlay } );
		decks.Add( new SpiritDeck { Type = SpiritDeck.DeckType.Discard, Cards = DiscardPile } );

		Elements = new ElementMgr( this );
		Forget = new ForgettingStrategy( this );
		Draw = new DrawCardStrategy( this );
	}

	public void InitSpirit( Board board, GameState gameState ){
		gameState.AddTimePassesAction(this);
		_gateway.DecisionMade += (d) => ActionScope.Current.Log(d);
		InitializeInternal(board,gameState);
		foreach(var init in Mods.OfType<IInitializeSpirit>().ToArray())
			init.Initialize();
	}

	public void AddCardToHand( PowerCard card ){
		Hand.Add(card);
	}

	#endregion

	#region Gateway stuff

	public IUserPortalPlus Portal => _gateway;



	public event Action<object?>? SelectionMade; // hook for: Reach Through the Efemeral Distance

	public void PreSelect( SpaceToken? st ) => _gateway.PreloadedSpaceToken = st;
	readonly UserGateway _gateway;

	#endregion

	#region Select

	// Required
	public async Task<T> SelectAlways<T>(string prompt, IEnumerable<T> options, bool autoSelectSingle = false) where T : class, IOption
		=> (await Select(prompt, options, autoSelectSingle ? Present.AutoSelectSingle : Present.Always))!;

	// Required - Don't use
	public async Task<T> SelectAlways<T>(A.TypedDecision<T> decision) where T : class, IOption => (await Select(decision))!;

	// Optional
	public Task<T?> Select<T>(string prompt, IEnumerable<T> options, Present present) where T : class, IOption {
		if(options is IEnumerable<Space>)
			options = options.OrderBy(o=>o.Text);
		return Select(new A.TypedDecision<T>(prompt, options.Distinct(), present));
	}

	// Optional
	public Task<T?> Select<T>(string prompt, IEnumerable<T> options, string cancelText) where T : class, IOption {
		return Select(new A.TypedDecision<T>(prompt, options.Distinct(), cancelText));
	}

	// Optional
	public async Task<T?> Select<T>(A.TypedDecision<T> decision) where T : class, IOption {
		var selection = await _gateway.Select<T>(decision);
		SelectionMade?.Invoke(selection);
		return selection;
	}


	#endregion Select

	public ElementMgr Elements;
	public DrawCardStrategy Draw;
	public ForgettingStrategy Forget;

	#region Growth

	public GrowthTrack GrowthTrack { get; set; }

	public bool HasMoreGrowthActions => GetAvailableActions( Phase.Growth ).OfType<IHelpGrowActionFactory>().Any()
		|| GrowthTrack.RemainingOptions( Energy ).SelectMany( o => o.GrowthActionFactories ).Any();

	/// <summary>
	/// Selects and resolves exactly one Growth action - safe to call repeatedly, one decision at a time,
	/// with a JSON save/restore in between each call (see docs/GameSerialization-Roadmap.md's mid-action
	/// note - this is the one boundary-safe unit of work for the Growth phase). GrowthTrack.Reset() must
	/// have already run once for the round (DoGrowth below does this for the common "run the whole phase"
	/// case).
	/// </summary>
	public async Task SelectAndResolveNextGrowthAction() {
		bool isNewPick = !GetAvailableActions( Phase.Growth ).Any();

		// Mid-group (not isNewPick), only the already-committed remainder should be offered - not other
		// candidate groups - so remainingOptions stays empty until the current group is fully resolved.
		GrowthGroup[] remainingGroupOptions = isNewPick ? GrowthTrack.RemainingOptions( Energy ) : [];
		IActionFactory[] consolidated = remainingGroupOptions.SelectMany( grp => grp.GrowthActionFactories )
			.Union( GetAvailableActions( Phase.Growth ) ).ToArray();

		IActionFactory selectedAction = await SelectAlways( new A.GrowthDecision( "Select Growth", consolidated, Present.Always ) );

		GrowthGroup? newGroup = isNewPick
			? remainingGroupOptions.SingleOrDefault( grp => grp.GrowthActionFactories.Contains( selectedAction ) )
			: null;

		if( newGroup is not null ) {
			// Mark as used and queue up its GrowthActions
			GrowthTrack.MarkAsUsed( newGroup );
			foreach( IHelpGrowActionFactory action in newGroup.GrowthActionFactories )
				_availableActions.Add( action );
			// Run any other Auto-actions
			foreach( var autoAction in newGroup.GrowthActionFactories.Where( x => x.AutoRun && x != selectedAction ) )
				await ResolveActionAsync( autoAction, Phase.Growth );
		}

		// Run selected action
		await ResolveActionAsync( selectedAction, Phase.Growth );
	}

	/// <summary>
	/// Called once growth is complete.
	/// </summary>
	/// <returns></returns>
	public async Task EndGrowth() {
		await using ActionScope action2 = await ActionScope.StartSpiritAction( ActionCategory.Spirit_PresenceTrackIcon, this );
		await EndGrowth_Inner( this );
	}

	protected virtual async Task EndGrowth_Inner( Spirit self ) {

		// Energy
		Energy += EnergyPerTurn;
		await EnergyCollected.InvokeAsync(this);

		// Elements from the Presence Tracks were added when the round started.

		// Do actions AFTER energy and elements have been added - in case playing ManyMindsMoveAsOne - Pay 2 for power card.
		foreach(var action in Presence.RevealedActions)
			await action.ActAsync( self );

	}

	/// <summary>
	/// Selects and resolves exactly one action for the phase in gs.Phase - safe to call repeatedly, one
	/// decision at a time. Returns false once there's nothing left to do (no options remain, or the user
	/// declined/chose "Done"), signaling the caller to stop looping.
	/// </summary>
	public async Task<bool> SelectAndResolveNextAction( GameState gs ) {
		Phase phase = gs.Phase;
		Present present = phase switch{ Phase.Growth or Phase.Init => Present.Always, _ => Present.Done };

		IActionFactory[] options = GetAvailableActions(phase).ToArray();
		if( options.Length == 0 ) return false;

		IActionFactory? option = await Select(new A.TypedDecision<IActionFactory>("Select " + phase + " to resolve", options, present));
		if( option is null ) return false;

		await ResolveActionAsync(option, phase);
		return true;
	}

	#endregion

	// Events
	public AsyncEvent<Spirit> EnergyCollected = new AsyncEvent<Spirit>();
	public ModBucket Mods = new ModBucket();

	// Recorded by GameBuilder.Build1Spirit as each aspect is applied - nothing else currently remembers
	// which aspects a spirit was built with. Not yet consumed by anything; kept for a future Spirit.Mods
	// restore-by-replay path (docs/GameSerialization-Roadmap.md).
	public List<AspectConfigKey> AppliedAspects = [];

	#region Cards

	public List<PowerCard> Hand = [];	    // in hand
	public List<PowerCard> InPlay = [];		// paid for / played
	public List<PowerCard> DiscardPile = []; // Cards are not transferred to discard pile until end of turn because we need to keep track of their elements.

	public SpiritDeck[] Decks => [.. decks];

	// This is not part of Decks because these cards are transient and don't actually belong to the spirit.
	public List<PowerCard> DraftDeck = [];   // The deck the cards are in while they are being drawn/drafted

	readonly protected List<SpiritDeck> decks = [];

	readonly List<IActionFactory> _availableActions = [];
	readonly List<IActionFactory> _usedActions = [];
	readonly protected List<InnatePower> _usedInnates = [];

	// so spirits can replay used cards or collect them instead of discard
	public IEnumerable<IActionFactory> UsedActions => _usedActions;

	public void Reclaim( PowerCard reclaimCard ) {
		ArgumentNullException.ThrowIfNull( reclaimCard );
		if(!DiscardPile.Remove( reclaimCard )) 
			if(InPlay.Contains( reclaimCard )) {
				Forget.RemoveCardFromPlay( reclaimCard );
			} else
				throw new InvalidOperationException( "Can't find the card to reclaim. Not in dicard nor in play" );
		Hand.Add( reclaimCard );
	}

	public async Task Reclaim1FromDiscard() {
		PowerCard? cardToReclaim = await this.SelectPowerCard( "Select card to reclaim.", 1, DiscardPile, CardUse.Reclaim, Present.Always );
		if( cardToReclaim is not null )
			Reclaim( cardToReclaim );
	}

	/// <summary>
	/// Called mid-round.  If reclaiming card from hand, looses elements
	/// </summary>
	public async Task Reclaim1FromDiscardOrPlayed() {
		PowerCard? cardToReclaim = await this.SelectPowerCard( "Select card to reclaim.", 1, DiscardPile.Union(InPlay), CardUse.Reclaim, Present.Always );
		if(cardToReclaim is not null )
			Reclaim( cardToReclaim );
	}

	#endregion

	#region (Unresolved) Actions 

	public IEnumerable<IActionFactory> GetAvailableActions(Phase phase) {

		var activatableList = AllActions
			.Where(action => action.CouldActivateDuring(phase, this))
			.ToList();

		// Modify the list of actions that are available
		foreach( IModifyAvailableActions x in Mods.OfType<IModifyAvailableActions>() )
			x.Modify(activatableList,phase);

		return activatableList;
	}

	/// <summary>
	/// All Unfiltered Innates + AvailableActions
	/// </summary>
	public IEnumerable<IActionFactory> AllActions {
		get {
			foreach( IActionFactory action in _availableActions )
				yield return action;

			foreach( InnatePower innate in InnatePowers )
				if( !InnateWasUsed(innate) )
					yield return innate;
		}
	}

	public bool InnateWasUsed(InnatePower innate) => _usedInnates.Contains(innate);

	public void AddActionFactory( IActionFactory factory ) {
		_availableActions.Add( factory );
	}

	/// <summary>
	/// Starts ActionScope, marks as Resolved, Resolves, checks win/los
	/// </summary>
	public async Task ResolveActionAsync(IActionFactory factory, Phase phase) {

		await using ActionScope actionScope = await ActionScope.StartSpiritAction( GetActionCategoryForSpiritAction(phase), this );
		MarkAsResolved( factory ); // removing first, so action can restore it if desired
		await factory.ActivateAsync( this );

		// Send event
		foreach(var x in Mods.OfType<IHandleActivatedActions>() )
			x.ActionActivated(factory);

		GameState.Current.CheckWinLoss();
	}

	/// <summary>
	/// Removes it from the Unresolved-list
	/// </summary>
	// !!! Make this protected once we figure out how Fractured Day's Growth work without access to this.
	public void MarkAsResolved(IActionFactory selectedActionFactory) {

		_usedActions.Add(selectedActionFactory);

		int index = _availableActions.IndexOf(selectedActionFactory);
		if( index != -1 ) {
			_availableActions.RemoveAt(index);
			return;
		}

		if( selectedActionFactory is InnatePower ip ) { // Could reverse this and instead of listing the used, create a not-used list that we remove them from when used
			_usedInnates.Add(ip);
			return;
		}

	}

	static ActionCategory GetActionCategoryForSpiritAction( Phase phase ) {
		return phase switch {
			Phase.Init or
			Phase.Growth => ActionCategory.Spirit_Growth,
			Phase.Fast or
			Phase.Slow => ActionCategory.Spirit_Power, // ! this is wrong. Some Special rules add actions during fast & slow that aren't Spirit powers. (Stranded)
			_ => throw new InvalidOperationException(),
		};
	}


	#endregion (Unresolved) Actions

	#region presence

	/// <summary> # of coins in the bank. </summary>
	public int Energy { 
		get => _energy;
		set => _energy = value > 0 ? value : 0; // Be safe - Erosion of Fear tries to make this negative.
	}
	int _energy;

	public SpiritPresence Presence {get;set;}
	public Incarna Incarna => Presence.Incarna; // convenience shortcut

	/// <summary> Energy gain per turn </summary>
	public int EnergyPerTurn => Presence.EnergyPerTurn;
	public virtual int NumberOfCardsPlayablePerTurn => Presence.CardPlayPerTurn + TempCardPlayBoost;

	public int TempCardPlayBoost = 0;

	#endregion

	protected abstract void InitializeInternal( Board board, GameState gameState );

	#region Bind / Target helpers

	/// <summary> Performs any action that is required at beginning of your action </summary>
	public virtual void InitSpiritAction( ActionScope scope ) {
		foreach(var configurer in Mods.OfType<IConfigureMyActions>())
			configurer.Configure(this,scope);
	}

	/// <summary> Convenience method only. calls new SelfCtx(this) </summary>
	public TargetSpaceCtx Target( SpaceSpec space ) => new TargetSpaceCtx( this, space );
	public TargetSpaceCtx Target( Space space ) => new TargetSpaceCtx(this, space.SpaceSpec);
	public TargetSpiritCtx Target( Spirit spirit ) => new TargetSpiritCtx( this, spirit );
	public void ResetTargetter() => _targetter = null;
	public Targetter Targetter {
		get => _targetter ??= new Targetter(this);
		set => _targetter = value;
	}
	Targetter? _targetter;

	#region Temporary - Fear - until we can find a better home for it.

	public virtual void AddFear( int count ) {
		GameState.Current.Fear.Add( count );
	}

	#endregion Temporary - Fear - until we can find a better home for it.

	public bool ActionIsMyPower {
		get {
			var scope = ActionScope.Current;
			return scope.Category == ActionCategory.Spirit_Power 
				&& scope.Owner == this;
		}
	}

	#endregion Bind helpers

	#region IRunWhenTimePasses imp

	bool IRunWhenTimePasses.RemoveAfterRun => false;

	public virtual Task TimePasses( GameState gameState ) {
		// reset cards / powers
		DiscardPile.AddRange( InPlay );
		InPlay.Clear();
		_availableActions.Clear();
		_usedActions.Clear();
		_usedInnates.Clear();

		// Card plays
		TempCardPlayBoost = 0;

		// Elements
		InitElementsFromPresence();

		// Roll back a temporary PowerRangeCalc override (one with a .Previous) at end of round - see
		// the PowerRangeCalc setter's remarks below.
		if( _powerRangeCalc.Previous is not null ) {
			ICalcRange cur = _powerRangeCalc;
			while( cur.Previous is not null )
				cur = cur.Previous;
			_powerRangeCalc = cur;
		}

		var cleanupMods = Mods.OfType<ICleanupSpiritWhenTimePasses>().ToArray();
		foreach(var mod in cleanupMods ) mod.CleanupSpirit(this);
		foreach(var mod in Mods.OfType<IEndWhenTimePasses>().ToArray()) Mods.Remove((ISpiritMod)mod);
		return Task.CompletedTask;
	}
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	#endregion IRunWhenTimePasses imp

	public void InitElementsFromPresence() {
		Elements.Init( Presence.TrackElements );
	}

	#region Play Cards


	/// <summary> Plays card from hand for cost. </summary>
	public Task SelectAndPlayCardsFromHand( int? numberToPlay = null) {
		return SelectAndPlayCardsFromHand_Inner( numberToPlay ?? NumberOfCardsPlayablePerTurn );
	}

	protected virtual async Task SelectAndPlayCardsFromHand_Inner( int remainingToPlay ) {
		PowerCard[] powerCardOptions;
		while(0 < remainingToPlay
			&& 0 < (powerCardOptions = GetPowerCardOptions()).Length
			&& await SelectAndPlay1( powerCardOptions, remainingToPlay )
		)
			--remainingToPlay;

		PowerCard[] GetPowerCardOptions() => [.. Hand.Where(c => c.Cost <= Energy)];
	}

	async Task<bool> SelectAndPlay1( PowerCard[] powerCardOptions, int remainingToPlay ) {
		string prompt = $"Play power card (${Energy} / {remainingToPlay})";
		PowerCard? card = await this.SelectPowerCard( prompt, remainingToPlay, powerCardOptions, CardUse.Play, Present.Done );
		if(card is null) return false;

		PlayCard( card );

		foreach( var mod in Mods.OfType<IHandleCardPlayed>() )
			await mod.Handle(this,card);

		return true;
	}

	public void PlayCard( PowerCard card, int? cost = null ) {
		if(!cost.HasValue) cost = card.Cost;

		if(!Hand.Contains( card )) 
			throw new CardNotAvailableException();
		if(Energy < cost) throw new InsufficientEnergyException();

		Hand.Remove( card );
		InPlay.Add( card );
		Energy -= cost.Value;
		Elements.Add(card.Elements);

		AddActionFactory( card );
	}

	#endregion

	#region Save/Load Memento

	object IHaveMemento.Memento {
		get => new Memento(this);
		set => ((Memento)value).Restore(this);
	}

	// Whatever this returns, get saved to the memento
	protected virtual object? CustomMementoValue {
		get { return null; }
		set { }
	}

	protected class Memento( Spirit _spirit ) {
		public void Restore(Spirit spirit) {
			((IHaveMemento)spirit.Presence).Memento = _presence;
			((IHaveMemento)spirit.EnergyCollected).Memento = _energyCollectedHooks;

			spirit.GrowthTrack = _growth;
			spirit.InnatePowers = _innates;
			spirit.Energy = _energy;
			spirit.TempCardPlayBoost = _tempCardPlayBoost;
			InitFromArray( spirit.Elements.Elements, _elements); // spirit.InitElementsFromPresence();

			spirit.Hand.SetItems( _hand );
			spirit.InPlay.SetItems( _purchased );
			spirit.DiscardPile.SetItems( _discarded );

			spirit.TargetingSourceStrategy = _targetingSourceStrategy;
			spirit.PowerRangeCalc = _powerRangeCalc;

			spirit._availableActions.SetItems( _available );
			spirit._usedActions.SetItems( _usedActions );
			spirit._usedInnates.SetItems( _usedInnates );

			spirit.CustomMementoValue = _tag;
			spirit.BonusDamage = _bonusDamage;
		}
		readonly GrowthTrack _growth = _spirit.GrowthTrack;
		readonly InnatePower[] _innates = _spirit.InnatePowers;

		readonly int _energy = _spirit.Energy;
		readonly int _tempCardPlayBoost = _spirit.TempCardPlayBoost;
		readonly KeyValuePair<Element,int>[] _elements = [.. _spirit.Elements.Elements];
		readonly object _presence = ((IHaveMemento)_spirit.Presence).Memento;

		readonly PowerCard[] _hand = [.. _spirit.Hand];
		readonly PowerCard[] _purchased = [.. _spirit.InPlay];
		readonly PowerCard[] _discarded = [.. _spirit.DiscardPile];

		readonly ITargetingSourceStrategy _targetingSourceStrategy = _spirit.TargetingSourceStrategy;
		readonly ICalcRange _powerRangeCalc = _spirit.PowerRangeCalc;

		readonly IActionFactory[] _available = [.. _spirit._availableActions];
		readonly IActionFactory[] _usedActions = [.. _spirit._usedActions];
		readonly InnatePower[] _usedInnates = [.. _spirit._usedInnates];

		readonly object _energyCollectedHooks = ((IHaveMemento)_spirit.EnergyCollected).Memento;
		readonly int _bonusDamage = _spirit.BonusDamage;
		readonly object? _tag = _spirit.CustomMementoValue;
	}
	static public void InitFromArray( CountDictionary<Element> dict, KeyValuePair<Element, int>[] array ) {
		dict.Clear();
		foreach(var p in array) dict[p.Key] = p.Value;
	}

	#endregion

	#region Json

	/// <summary>
	/// Named keys. Assumes the target Spirit was already reconstructed via the normal MakeSpirit +
	/// aspect-application pipeline (real game setup, and how PowerCardRegistry/InnatePowerRegistry seed themselves - see
	/// docs/GameSerialization-Roadmap.md section 4) *before* RestoreFromJson is called - it only
	/// overlays the runtime-mutable deltas on top, the same "construct through the normal path, then
	/// overwrite fields" approach Fear/Island/BlightCard's FromJson already use.
	///
	/// Deliberately NOT captured, because they're spirit-type/aspect data - identical every time the
	/// same concrete Spirit subtype + aspect selection is (re)constructed, not a runtime delta:
	/// - GrowthTrack's Groups/PickGroups structure / InnatePowers: aspects mutate these once during
	///   ModSpirit (e.g. Tactician, SpreadingHostility, Pandemonium's innate swap) - re-running that setup
	///   reproduces them, same "re-run setup" approach Mods (below) also relies on for its Low tier.
	///   GrowthTrack.Used *is* a real per-round runtime delta though (which GrowthGroups have already
	///   been picked) - captured separately below via GrowthTrack.ToJson/RestoreFromJson.
	/// - EnergyCollected / SelectionMade - wiring, not state (section 11); re-subscribed by replaying
	///   InitSpirit/InitAspect, not serialized.
	///
	/// Closed gap: TargetingSourceStrategy/PowerRangeCalc used to only round-trip the DefaultXxx
	/// singleton case. An active *temporary* override round-trips via TargetingSourceStrategyRegistry/
	/// RangeCalcRegistry (EntwinedPower's EntwinedPresenceSource, Locus of the Serpent's Regard's
	/// IncludeSerpentsIncarna, and the 4 named ICalcRange decorators with a non-null Previous:
	/// RangeExtender, IncludeALandRangeCalculator, SkyStretchesToShoreApi, ExtendRange1FromMountain). A
	/// mod-assigned *persistent* non-default calculator's identity is still expected to already be
	/// correct post-replay (same as GrowthTrack above) rather than serialized directly - but where one
	/// also carries its own extra runtime state (ReachThroughEphemeralDistance's _usedThisRound), it
	/// registers its own RangeCalcRegistry tag purely to carry that state, resolving back to the
	/// already-replayed instance rather than constructing a fresh one (see its own ToJson remarks) -
	/// same reasoning as the Mods bucket immediately below.
	///
	/// Mods (docs/ISpiritMod-Types.md): most entries need nothing here at all - constructing the Spirit
	/// (or applying its aspect) already deterministically re-adds them, the same "replay is safe" logic
	/// as GrowthTrack/InnatePowers above (the catalog's Low tier, verified empirically by
	/// SpiritMods_LowTier_Tests). `mods` (ModsToJson/RestoreModsFromJson below) captures only the ones
	/// that also carry small extra state on top (the catalog's Medium tier) via ISerializableSpiritMod -
	/// SpiritModRegistry's own doc comment explains why most readers mutate an already-present instance
	/// rather than constructing a new one. Still open: the catalog's High tier (MarkedBeastMover,
	/// UnrelentingStrides, PourDownPower's RepeatLandCardForCost, IntensifyAirWater) - these track "used"
	/// by reference-equality against their own live instance inside _usedActions/AllActions below, so
	/// restoring them correctly needs Mods and _usedActions/_availableActions resolved in a coordinated
	/// order, not just independently reconstructed - see docs/ISpiritMod-Types.md's High tier plan.
	///
	/// Known gaps, documented rather than silently dropped:
	/// - _availableActions/_usedActions: PowerCard/InnatePower/GrowthAction/FastSlowAction resolve (the
	///   latter via SelfCmdRegistry, keyed off FastSlowAction.Cmd's own ISerializableSelfCmd - only
	///   PlayCardForCost implements it, the only IActOn&lt;Spirit&gt; FastSlowAction ever wraps
	///   solution-wide). Beyond those four, ISerializableActionFactory/ActionFactoryRegistry (same
	///   tag-dispatch shape) is the general extension point for types added directly via
	///   AddActionFactory - RepeatCardForCost, RepeatCheapestCardForCost, RepeatSpecificCardForCost
	///   (JaggedEarth, wraps a PowerCard resolved via the existing PowerCardRegistry), RepeatCardForFree,
	///   RelentlessRepeater (NatureIncarnate, wraps a PowerCard + a live Space resolved via
	///   ISerializationContext.Tokens/SpaceSpecByLabel), and EmpoweredAbduct (NatureIncarnate, stateless -
	///   resolves to its own shared static Singleton, not a fresh instance, since
	///   EnableEmpoweredAbductMod's Mods entry compares spirit.UsedActions against that exact reference)
	///   all implement it.
	///   ResolveSlowDuringFast/ResolveSlowDuringFast_OrViseVersa/SharpFangs's setup action are the
	///   remaining AddActionFactory-based holdouts - still throw, but are all trivially name-resolvable
	///   (stateless/parameterless) whenever someone gets to them.
	///   A structurally different, harder bucket: MarkedBeastMover, TheBehemothRises, and PourDownPower's
	///   private RepeatLandCardForCost are never added via AddActionFactory at all - they're injected
	///   each round by an IModifyAvailableActions mod, and each owning mod tracks "was this used" by
	///   reference equality against its own cached singleton field (PourDownPower.UsedCounts,
	///   UnrelentingStrides.BehemothUsed, MarkedBeastMover's own "once per round" check) - the same
	///   reference-identity class of bug fixed for SpiritPresenceToken/_usedInnates elsewhere in this
	///   section. See the Mods paragraph above - this is exactly its still-open High tier.
	/// - CustomMementoValue (the in-memory undo mechanism) is deliberately NOT reused here - Mementos are
	///   slated for removal later, so CustomStateToJson/RestoreCustomStateFromJson below re-implement
	///   whatever each of the 7 known subclasses needs directly against their real fields, independent of
	///   CustomMementoValue's getter/setter and its nested Memento-holder classes.
	/// </summary>
	public JsonObject ToJson( ISerializationContext ctx ) => new JsonObject {
		["Presence"] = Presence.ToJson( ctx ),
		["Energy"] = Energy,
		["TempCardPlayBoost"] = TempCardPlayBoost,
		["Elements"] = SerializeElements( Elements.Elements ),
		["Hand"] = SerializeCards( Hand ),
		["InPlay"] = SerializeCards( InPlay ),
		["DiscardPile"] = SerializeCards( DiscardPile ),
		["TargetingSource"] = TargetingSourceStrategy.ToJson( ctx ),
		["PowerRangeCalc"] = PowerRangeCalc.ToJson( ctx ),
		["GrowthUsed"] = GrowthTrack.ToJson( ctx ),
		["AvailableActions"] = SerializeActionFactories( _availableActions, ctx ),
		["UsedActions"] = SerializeActionFactories( _usedActions, ctx ),
		["UsedInnates"] = new JsonArray( _usedInnates.Select( ip => (JsonNode)ip.ToJson() ).ToArray() ),
		["BonusDamage"] = BonusDamage,
		["CustomState"] = CustomStateToJson( ctx ),
		["Mods"] = ModsToJson( ctx )
	};

	/// <summary> Restores onto an already-reconstructed Spirit - see ToJson's remarks. </summary>
	public void RestoreFromJson( JsonObject json, ISerializationContext ctx ) {
		Presence.RestoreFromJson( (JsonArray)json["Presence"]!, ctx );
		Energy = json["Energy"]!.GetValue<int>();
		TempCardPlayBoost = json["TempCardPlayBoost"]!.GetValue<int>();
		InitFromArray( Elements.Elements, DeserializeElements( (JsonArray)json["Elements"]! ) );

		Hand.SetItems( DeserializeCards( (JsonArray)json["Hand"]! ) );
		InPlay.SetItems( DeserializeCards( (JsonArray)json["InPlay"]! ) );
		DiscardPile.SetItems( DeserializeCards( (JsonArray)json["DiscardPile"]! ) );

		TargetingSourceStrategy = TargetingSourceStrategyRegistry.Deserialize( (JsonArray)json["TargetingSource"]!, ctx );
		PowerRangeCalc = RangeCalcRegistry.Deserialize( (JsonArray)json["PowerRangeCalc"]!, ctx );
		GrowthTrack.RestoreFromJson( (JsonArray)json["GrowthUsed"]!, ctx );

		_availableActions.SetItems( DeserializeActionFactories( (JsonArray)json["AvailableActions"]!, this, ctx ) );
		_usedActions.SetItems( DeserializeActionFactories( (JsonArray)json["UsedActions"]!, this, ctx ) );
		_usedInnates.SetItems( ( (JsonArray)json["UsedInnates"]! ).Select( n => ResolveInnate( (JsonArray)n!, this ) ).ToArray() );

		BonusDamage = json["BonusDamage"]!.GetValue<int>();
		RestoreCustomStateFromJson( json["CustomState"], ctx );
		RestoreModsFromJson( (JsonArray)json["Mods"]!, ctx );
	}

	/// <summary>
	/// Only mods implementing ISerializableSpiritMod are captured - see its own remarks. Everything else
	/// in Mods (docs/ISpiritMod-Types.md's Low tier) is expected to already be correct after whatever
	/// reconstructs this Spirit replays its constructor/aspect selection, same reasoning as GrowthTrack/
	/// InnatePowers above - not serialized here, not a gap.
	/// </summary>
	JsonArray ModsToJson( ISerializationContext ctx ) => new JsonArray(
		Mods.OfType<ISerializableSpiritMod>().Select( m => (JsonNode)m.ToJson( ctx ) ).ToArray() );

	void RestoreModsFromJson( JsonArray json, ISerializationContext ctx ) {
		foreach( JsonNode? n in json ) SpiritModRegistry.Restore( this, (JsonArray)n!, ctx );
	}

	/// <summary>
	/// Hook for the 7 known Spirit subclasses with extra state beyond the fields above (previously only
	/// tracked via CustomMementoValue's in-memory-only Memento) - FinderOfPathsUnseen (GatewayToken),
	/// FracturedDaysSplitTheSky (RNG replay position/Time/Days-That-Never-Were decks),
	/// ShiftingMemoryOfAges (prepared elements), WoundedWatersBleeding (healing markers/seek-healing
	/// flag), EmberEyedBehemoth (GrowthTrack group count), DancesUpEarthquakes (impending cards/energy).
	/// No-op by default. Deliberately independent of CustomMementoValue - see the ToJson remarks above.
	/// </summary>
	protected virtual JsonNode? CustomStateToJson( ISerializationContext ctx ) => null;

	/// <summary> Restore-side counterpart to CustomStateToJson - see its remarks. </summary>
	protected virtual void RestoreCustomStateFromJson( JsonNode? json, ISerializationContext ctx ) { }

	/// <summary> protected, not private - ShiftingMemoryOfAges' CustomStateToJson reuses this for its own
	/// PreparedElements dictionary (same CountDictionary&lt;Element&gt; shape as Elements itself). </summary>
	protected static JsonArray SerializeElements( CountDictionary<Element> elements ) => new JsonArray(
		elements.Select( p => (JsonNode)new JsonArray( p.Key.ToString(), p.Value ) ).ToArray() );

	protected static KeyValuePair<Element, int>[] DeserializeElements( JsonArray json ) => json
		.Select( n => {
			var pair = (JsonArray)n!;
			return new KeyValuePair<Element, int>( Enum.Parse<Element>( pair[0]!.GetValue<string>() ), pair[1]!.GetValue<int>() );
		} )
		.ToArray();

	static JsonArray SerializeCards( IEnumerable<PowerCard> cards ) => new JsonArray(
		cards.Select( c => (JsonNode)c.ToJson() ).ToArray() );

	static PowerCard[] DeserializeCards( JsonArray json ) => json
		.Select( n => PowerCardRegistry.Deserialize( n! ) )
		.ToArray();

	JsonArray SerializeActionFactories( IEnumerable<IActionFactory> factories, ISerializationContext ctx ) => new JsonArray(
		factories.Select( f => (JsonNode)SerializeActionFactory( f, ctx ) ).ToArray() );

	/// <summary>
	/// Checks every Mods entry that owns cached IActionFactory instances (docs/ISpiritMod-Types.md's
	/// High tier) before falling through to the ordinary cases below - see IOwnedActionFactories'
	/// own remarks for why this has to come first.
	/// </summary>
	JsonArray SerializeActionFactory( IActionFactory factory, ISerializationContext ctx ) {
		foreach( IOwnedActionFactories owner in Mods.OfType<IOwnedActionFactories>() ) {
			string? key = owner.KeyFor( factory );
			if( key is not null ) return new JsonArray( "ModOwned", owner.ModTag, key );
		}
		return factory switch {
			PowerCard card => new JsonArray( "Card", card.ToJson() ),
			InnatePower innate => new JsonArray( "Innate", innate.ToJson() ),
			GrowthAction growth => SerializeGrowthAction( growth, ctx ),
			FastSlowAction fastSlow => new JsonArray( "FastSlow", SelfCmdRegistry.Serialize( fastSlow.Cmd, ctx ) ),
			ISerializableActionFactory custom => custom.ToJson( ctx ),
			_ => throw new NotSupportedException( $"Spirit.ToJson doesn't support IActionFactory of type {factory.GetType().Name} yet - only PowerCard/InnatePower/GrowthAction/FastSlowAction/ISerializableActionFactory implementers are resolvable (see docs/GameSerialization-Roadmap.md section 4's 'Also out of scope' note)." )
		};
	}

	/// <summary>
	/// Identifies a GrowthAction by its position in GrowthTrack where possible - GrowthAction instances
	/// cached per GrowthGroup (GrowthGroup.GrowthActionFactories) aren't registered anywhere, so position
	/// is the only stable identity available for those. Some GrowthActions are never part of
	/// GrowthTrack at all though - one-off instances built with `.ToGrowth()` and added directly via
	/// AddActionFactory during spirit setup (e.g. SharpFangs.SetupAction, Ocean/Volcano/FinderOfPathsUnseen's
	/// setup actions) - for those, falls back to "GrowthCmd", resolving the wrapped Cmd via
	/// SelfCmdRegistry (same registry FastSlowAction uses) plus the captured Phase. Requires the wrapped
	/// SpiritAction to be a named ISerializableSelfCmd subclass (Locus.PlaceIncarnaAndFireEnergy,
	/// WarriorSpiritsRaidingParty.PlaceIncarna, Lair.InitLair) rather than an anonymous
	/// `new SpiritAction(title, delegate)` - an inline lambda/method-group delegate has nothing to key a
	/// JSON round-trip off of.
	/// </summary>
	JsonArray SerializeGrowthAction( GrowthAction growth, ISerializationContext ctx ) {
		var pickGroups = GrowthTrack.PickGroups;
		for( int p = 0; p < pickGroups.Count; ++p ) {
			GrowthGroup[] groups = pickGroups[p].Groups;
			for( int g = 0; g < groups.Length; ++g ) {
				int a = Array.IndexOf( groups[g].GrowthActionFactories, growth );
				if( a != -1 ) return new JsonArray( "Growth", p, g, a );
			}
		}
		return new JsonArray( "GrowthCmd", (int)growth.Phase, SelfCmdRegistry.Serialize( growth.Cmd, ctx ) );
	}

	static IActionFactory[] DeserializeActionFactories( JsonArray json, Spirit spirit, ISerializationContext ctx ) => json
		.Select( n => DeserializeActionFactory( (JsonArray)n!, spirit, ctx ) )
		.ToArray();

	static IActionFactory DeserializeActionFactory( JsonArray json, Spirit spirit, ISerializationContext ctx ) => json[0]!.GetValue<string>() switch {
		"Card" => PowerCardRegistry.Deserialize( json[1]! ),
		"Innate" => ResolveInnate( (JsonArray)json[1]!, spirit ),
		"Growth" => spirit.GrowthTrack.PickGroups[json[1]!.GetValue<int>()].Groups[json[2]!.GetValue<int>()].GrowthActionFactories[json[3]!.GetValue<int>()],
		"GrowthCmd" => new GrowthAction( SelfCmdRegistry.Deserialize( (JsonArray)json[2]!, ctx ), (Phase)json[1]!.GetValue<int>() ),
		"FastSlow" => new FastSlowAction( SelfCmdRegistry.Deserialize( (JsonArray)json[1]!, ctx ) ),
		// Resolves through the already-replayed Mods entry itself (see IOwnedActionFactories), not a
		// fresh instance - same reasoning as SpiritModRegistry.
		"ModOwned" => spirit.Mods.OfType<IOwnedActionFactories>().Single( o => o.ModTag == json[1]!.GetValue<string>() ).ResolveActionFactory( json[2]!.GetValue<string>() ),
		_ => ActionFactoryRegistry.Deserialize( json, ctx )
	};

	/// <summary>
	/// Resolves by Title against the target spirit's own (reconstruction-assumed-identical)
	/// InnatePowers array, NOT InnatePowerRegistry - unlike PowerCard (fully immutable, reused
	/// everywhere via the registry with no reference-identity consequences), InnatePower references
	/// in _usedInnates must be reference-equal to the entries in Spirit.InnatePowers, because
	/// AllActions/InnateWasUsed compare them by reference (List.Contains). The registry's singleton
	/// comes from a *different* MakeSpirit() call (its own module-initializer seeding one), so using
	/// it here would silently make a used innate look unused again after restore.
	/// </summary>
	static InnatePower ResolveInnate( JsonArray json, Spirit spirit ) {
		string title = json[0]!.GetValue<string>();
		return spirit.InnatePowers.First( ip => ip.Title == title );
	}

	#endregion

	#region Targeting / Range

	/// <summary>
	/// Finds Spaces within Range of Spirits Presence
	/// </summary>
	/// <remarks>
	/// a) Selects correct range calculator (Default or Power)
	/// b) provides PresenceLands
	/// </remarks>
	// Used by:
	//		PlacePresence - Growth + Powers
	//		AddDestroyedPresence - mostly powers, but also 2 Growth
	//		Unrelenting Growth - Power
	//		Bargains of Power - Power
	//		Unleash a torrent - Power
	public Space[] FindSpacesWithinRange( TargetCriteria targetCriteria ) {
		ICalcRange rangeCalculator = ActionIsMyPower ? PowerRangeCalc : NonPowerRangeCalc;
		return rangeCalculator.GetTargetingRoute_MultiSpace( Presence.Lands, targetCriteria ).Targets;
	}

	/// <summary> 
	/// Calculates Source for *TARGETING* *Powers* only.  
	/// Don't use it for non-power calculations.
	/// Don't use it for non-targeting Range-only calculations.
	/// </summary>
	public ITargetingSourceStrategy TargetingSourceStrategy = new DefaultPowerSourceStrategy();

	/// <summary> Calculates the Range for *Powers* only.  Don't use it for non-power calculations. </summary>
	/// <remarks> RangeCals with .Previous are automatically rolled back at end of Round - see TimePasses above.</remarks>
	public ICalcRange PowerRangeCalc {
		get => _powerRangeCalc;
		set => _powerRangeCalc = value;
	}
	ICalcRange _powerRangeCalc = DefaultRangeCalculator.Singleton;

	public ICalcRange NonPowerRangeCalc = DefaultRangeCalculator.Singleton;

	#endregion

	// Works like badlands.
	public int BonusDamage { get; set; } // This is a hack for Flame's Fury & Earth Moves With Vigor And Might
	// !!! Make sure this is included in all Damage paths.  Make sure nothing skips over it by going directly to Space.DamageInvaders(...)

	Spirit IHaveASpirit.Self => this;

}

public interface IHaveSecondaryElements {
	CountDictionary<Element> SecondaryElements { get; }
}

public enum ECouldHaveElements { No, Yes, AsPrepared }
