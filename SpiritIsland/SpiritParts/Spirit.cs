#nullable enable
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
	}

	public void AddCardToHand( PowerCard card ){
		Hand.Add(card);
	}

	#endregion

	#region Gateway stuff

	public IUserPortalPlus Portal => _gateway;

	/// <summary> Use ONLY for decisions that will ALWAYS return an option. </summary>
	public async Task<T> SelectAlwaysAsync<T>(A.TypedDecision<T> decision) where T : class, IOption => (await SelectAsync(decision))!;

	public async Task<T?> SelectAsync<T>( A.TypedDecision<T> decision ) where T : class, IOption {
		var selection = await _gateway.Select<T>(decision);
		SelectionMade?.Invoke(selection);
		return selection;
	}

	public event Action<object>? SelectionMade; // hook for: Reach Through the Efemeral Distance

	public void PreSelect( SpaceToken st ) => _gateway.PreloadedSpaceToken = st;
	readonly UserGateway _gateway;

	#endregion

	public ElementMgr Elements;
	public DrawCardStrategy Draw;
	public ForgettingStrategy Forget;

	#region Growth

	public GrowthTrack GrowthTrack { get; protected set; }

	/// <remarks>So we can init stuff at beginning of turn if we need to.</remarks>
	public virtual async Task DoGrowth(GameState gameState) {
		await new DoGrowthClass(this,gameState).Execute();
		await ApplyRevealedPresenceTrack();
	}

	async Task ApplyRevealedPresenceTrack() {
		await using ActionScope action2 = await ActionScope.StartSpiritAction( ActionCategory.Spirit_PresenceTrackIcon, this );
		await ApplyRevealedPresenceTracks_Inner( this );
	}

	protected virtual async Task ApplyRevealedPresenceTracks_Inner( Spirit self ) {

		// Energy
		Energy += EnergyPerTurn;
		await EnergyCollected.InvokeAsync(this);
		// ! Elements were added when the round started.

		// Do actions AFTER energy and elements have been added - in case playing ManyMindsMoveAsOne - Pay 2 for power card.
		foreach(var action in Presence.RevealedActions)
			await action.ActAsync( self );

	}

	/// <summary> Resolves Action for the current phase </summary>
	public async Task SelectAndResolveActions( GameState gs ) {
		Phase phase = gs.Phase;
		Present present = phase == Phase.Growth ? Present.Always : Present.Done;

		IActionFactory[] options = GetAvailableActions(phase).ToArray();
		while( 0 < options.Length ) {
			IActionFactory? option = await SelectAsync(new A.TypedDecision<IActionFactory>("Select " + phase + " to resolve", options, present));
			if( option is null ) break;

			await ResolveActionAsync(option, phase);

			// next
			options = GetAvailableActions(phase).ToArray();
		}

	}

	#endregion

	// Events
	public AsyncEvent<Spirit> EnergyCollected = new AsyncEvent<Spirit>();
	public List<ISpiritMod> Mods = [];

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
		PowerCard cardToReclaim = await this.SelectPowerCard( "Select card to reclaim.", 1, DiscardPile, CardUse.Reclaim, Present.Always );
		if( cardToReclaim != null )
			Reclaim( cardToReclaim );
	}

	/// <summary>
	/// Called mid-round.  If reclaiming card from hand, looses elements
	/// </summary>
	public async Task Reclaim1FromDiscardOrPlayed() {
		PowerCard cardToReclaim = await this.SelectPowerCard( "Select card to reclaim.", 1, DiscardPile.Union(InPlay), CardUse.Reclaim, Present.Always );
		Reclaim( cardToReclaim );
	}

	#endregion

	#region (Unresolved) Actions 

	public IEnumerable<IActionFactory> GetAvailableActions(Phase phase) {
		var activatable = AllActions
			.Where(action => action.CouldActivateDuring(phase, this))
			.ToList();
		foreach( IModifyAvailableActions x in Mods.OfType<IModifyAvailableActions>() )
			x.Modify(activatable,phase);
		return activatable;
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
		RemoveFromUnresolvedActions( factory ); // removing first, so action can restore it if desired
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
	public void RemoveFromUnresolvedActions(IActionFactory selectedActionFactory) {

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
	public virtual void InitSpiritAction( ActionScope scope ) { }

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

	public bool RemoveAfterRun => false;
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

		var cleanupMods = Mods.OfType<ICleanupSpiritWhenTimePasses>().ToArray();
		foreach(var mod in cleanupMods ) mod.CleanupSpirit(this);
		var endingMods = Mods.OfType<IEndWhenTimePasses>().ToArray();
		foreach(var mod in endingMods) Mods.Remove((ISpiritMod)mod);
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
	}

	async Task<bool> SelectAndPlay1( PowerCard[] powerCardOptions, int remainingToPlay ) {
		string prompt = $"Play power card (${Energy} / {remainingToPlay})";
		PowerCard card = await this.SelectPowerCard( prompt, remainingToPlay, powerCardOptions, CardUse.Play, Present.Done );
		if(card == null) return false;

		PlayCard( card );

		foreach( var mod in Mods.OfType<IHandleCardPlayed>() )
			await mod.Handle(this,card);

		return true;
	}

	// Helper for SelectAndPlayCardsFromHand
	PowerCard[] GetPowerCardOptions() => Hand
		.Where( c => c.Cost <= Energy )
		.ToArray();

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

	#region Power Plug-ins

	// Overriden by Trickster because it costs them presence
	public virtual async Task RemoveBlight( TargetSpaceCtx ctx, int count=1 ) {
		if(ctx.Blight.Any)
			await ctx.Blight.Remove( count );
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
	/// <remarks> RangeCals with .Previous will automaticaly be rolled back at end of Round.</remarks>
	public ICalcRange PowerRangeCalc { 
		get => _powerRangeCalc;
		set {
			_powerRangeCalc = value;
			if( _powerRangeCalc.Previous is not null ) {
				GameState.Current.AddTimePassesAction(TimePassesAction.Once((gs) => {
					ICalcRange cur = PowerRangeCalc;
					while( cur.Previous is not null )
						cur = cur.Previous;
					_powerRangeCalc = cur;
				}));
			}
		}
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