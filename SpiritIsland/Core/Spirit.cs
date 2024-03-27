namespace SpiritIsland;

public abstract partial class Spirit 
	: IOption
	, IHaveASpirit // so all 'contexts' can use the same Picker and have a spirit to do the decision making.
	, IRunWhenTimePasses
	, IHaveMemento 
{

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
	public Task<T> SelectAsync<T>( A.TypedDecision<T> decision ) where T : class, IOption => _gateway.Select<T>( decision );
	public void PreSelect( SpaceToken st ) => _gateway.PreloadedSpaceToken = st;
	readonly UserGateway _gateway;

	#endregion

	#region Elements

	public readonly ElementMgr Elements;

	public virtual ECouldHaveElements CouldHaveElements( CountDictionary<Element> subset ) => Elements.CouldContain(subset) ? ECouldHaveElements.Yes : ECouldHaveElements.No;
	public virtual Task<bool> HasElement( CountDictionary<Element> subset, string description ) => Elements.ContainsAsync(subset, description);

	/// <summary>
	/// Spirit determines which Option they can activate.
	/// </summary>
	/// <remarks>overriden by Shiftin Memories and Volcano</remarks>
	public virtual async Task<IDrawableInnateTier> SelectInnateTierToActivate( IEnumerable<IDrawableInnateTier> innateOptions ) {
		IDrawableInnateTier match = null;
		foreach(var option in innateOptions.OrderBy( o => o.Elements.Total ))
			if(await HasElement( option.Elements, "Innate Tier" ))
				match = option;
		return match;
	}

	#endregion

	#region Growth

	public GrowthTrack GrowthTrack { get; protected set; }

	/// <remarks>Virtual so we can init stuff at beginning of turn if we need to.</remarks>
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


	public AsyncEvent<Spirit> EnergyCollected = new AsyncEvent<Spirit>();

	public async Task ResolveActions( GameState gs ) {
		Phase phase = gs.Phase;

		while(GetAvailableActions( phase ).Any()
			&& await ResolveAction( phase )
		) { }

	}

	/// <summary> Resolves 1 Action for the given Phase </summary>
	public async Task<bool> ResolveAction( Phase phase ) {

		Present present = phase == Phase.Growth ? Present.Always : Present.Done;

		// -------------
		// Select Actions to resolve
		// -------------
		IActionFactory[] options = this.GetAvailableActions( phase ).ToArray();
		IActionFactory option = await this.SelectFactory( "Select " + phase + " to resolve", options, present );
		if(option == null)
			return false;

		// if user clicked a slow card that was made fast, // slow card won't be in the options
		if(!options.Contains( option ))
			// find the fast version of the slow card that was clicked
			option = options.Cast<IActionFactory>()
				.First( factory => factory == option );

		if(!options.Contains( option ))
			throw new Exception( "Dude! - You selected something that wasn't an option" );

		await TakeActionAsync( option, phase );
		return true;
	}


	#endregion

	#region Cards

	public List<PowerCard> Hand = [];	    // in hand
	public List<PowerCard> InPlay = [];		// paid for / played
	public List<PowerCard> DiscardPile = []; // Cards are not transferred to discard pile until end of turn because we need to keep track of their elements.

	public SpiritDeck[] Decks => decks.ToArray();

	// This is not part of Decks because these cards are transient and don't actually belong to the spirit.
	public List<PowerCard> DraftDeck = [];   // The virtual deck the cards are in while they are being drawn/drafted

	readonly protected List<SpiritDeck> decks = [];

	readonly List<IActionFactory> _availableActions = [];
	readonly HashSet<IActionFactory> _usedActions = [];
	readonly List<InnatePower>       _usedInnates = [];

	// so spirits can replay used cards or collect them instead of discard
	public IEnumerable<IActionFactory> UsedActions => _usedActions;

	public virtual IEnumerable<IActionFactory> GetAvailableActions(Phase speed) {
		foreach(var action in AvailableActions) {
			if( action.CouldActivateDuring( speed, this ) )
				yield return action;
		}
	}

	// Holds Fast and Slow actions,
	// depends on Fast/Slow phase to only select the actions that are appropriate
	protected IEnumerable<IActionFactory> AvailableActions { 
		get {
			foreach(IActionFactory action in _availableActions)
				yield return action;

			foreach(InnatePower innate in InnatePowers)
				if( !_usedInnates.Contains( innate ) )
					yield return innate;
		}
	}

	public void Reclaim( PowerCard reclaimCard ) {
		ArgumentNullException.ThrowIfNull( reclaimCard );
		if(!DiscardPile.Remove( reclaimCard )) 
			if(InPlay.Contains( reclaimCard )) {
				RemoveCardFromPlay( reclaimCard );
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

	/// <summary> Forget USER SELECTED power card. </summary>
	public virtual async Task<PowerCard> ForgetACard( IEnumerable<PowerCard> options = null, Present present = Present.Always ) {

		options ??= GetForgetableCards();

		PowerCard cardToForget = await this.SelectPowerCard( "Select power card to forget", 1, options, CardUse.Forget, present );
		if( cardToForget != null )
			ForgetThisCard( cardToForget );
		return cardToForget;
	}

	protected virtual IEnumerable<PowerCard> GetForgetableCards() 
		=> InPlay                 // in play
			.Union( Hand )        // in Hand
			.Union( DiscardPile ) // in Discard
			.ToArray();

	public virtual void ForgetThisCard( PowerCard cardToRemove ) {
		// A card can be in one of 3 places
		// (1) Purchased / Active
		if(InPlay.Contains( cardToRemove ))
			RemoveCardFromPlay( cardToRemove );
		// (2) Unpurchased, still in hand
		Hand.Remove( cardToRemove );
		// (3) used, discarded
		DiscardPile.Remove( cardToRemove );
	}

	void RemoveCardFromPlay( PowerCard cardToRemove ) {
		foreach(var el in cardToRemove.Elements)
			Elements.Remove(el.Key, el.Value);// lose elements from forgotten card
		InPlay.Remove( cardToRemove );
	}

	/// <summary>
	/// Removes it from the Unresolved-list
	/// </summary>
	public virtual void RemoveFromUnresolvedActions(IActionFactory selectedActionFactory ) {
		if(selectedActionFactory is InnatePower ip) { // Could reverse this and instead of listing the used, create a not-used list that we remove them from when used
			_usedInnates.Add( ip );
			return;
		}

		int index = _availableActions.IndexOf( selectedActionFactory );
		if(index == -1) 
			throw new InvalidOperationException( $"Unable to remove ActionFactory {selectedActionFactory.Title} from Unresolved Actions because it is not there." );

		_usedActions.Add(_availableActions[index]);
		_availableActions.RemoveAt( index );

	}

	public void AddActionFactory( IActionFactory factory ) {
		// if we are manually restoring an innate power - like EEB
		if(_usedInnates.Contains( factory )) {
			_usedInnates.Remove((InnatePower)factory);
			return;
		}

		_availableActions.Add( factory );
		_usedActions.Remove( factory ); // incase this is was a repeat
	}

	public virtual async Task TakeActionAsync(IActionFactory factory, Phase phase) {
		await using ActionScope actionScope = await ActionScope.StartSpiritAction( GetActionCategoryForSpiritAction(phase), this );  
			
		RemoveFromUnresolvedActions( factory ); // removing first, so action can restore it if desired
		await factory.ActivateAsync( this );
		GameState.Current.CheckWinLoss(); // @@@
	}

	static ActionCategory GetActionCategoryForSpiritAction( Phase phase ) {
		return phase switch {
			Phase.Init or
			Phase.Growth => ActionCategory.Spirit_Growth,
			Phase.Fast or
			Phase.Slow => ActionCategory.Spirit_Power,
			_ => throw new InvalidOperationException(),
		};
	}


	#endregion

	#region presence

	/// <summary> # of coins in the bank. </summary>
	public int Energy { 
		get => _energy;
		set => _energy = value > 0 ? value : 0; // Be safe - Erosion of Fear tries to make this negative.
	}
	int _energy;

	public SpiritPresence Presence {get;}
	public Incarna Incarna => Presence.Incarna; // convenience shortcut

	/// <summary> Energy gain per turn </summary>
	public int EnergyPerTurn => Presence.EnergyPerTurn;
	public virtual int NumberOfCardsPlayablePerTurn => Presence.CardPlayPerTurn + TempCardPlayBoost;

	public int TempCardPlayBoost = 0;


	#endregion

	string IOption.Text => SpiritName;

	public abstract string SpiritName { get; }

	abstract public SpecialRule[] SpecialRules { get; }

	public virtual InnatePower[] InnatePowers { get; set; } = [];

	protected abstract void InitializeInternal( Board board, GameState gameState );

	#region Bind / Target helpers

	public virtual void InitSpiritAction( ActionScope scope ) { }

	/// <summary> Convenience method only. calls new SelfCtx(this) </summary>
	public TargetSpaceCtx Target( SpaceSpec space ) => new TargetSpaceCtx( this, space );
	public TargetSpaceCtx Target(Space space) => new TargetSpaceCtx(this, space.SpaceSpec);
	public TargetSpiritCtx Target( Spirit spirit ) => new TargetSpiritCtx( this, spirit );

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

		return Task.CompletedTask;
	}
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	#endregion IRunWhenTimePasses imp

	public void InitElementsFromPresence() {
		Elements.Init( Presence.TrackElements );
	}

	// pluggable, draw power card
	#region Draw Card

	public async Task<DrawCardResult> Draw(Func<PowerCardDeck,Task<bool>> forgetCardForMajor = null) {
		PowerCardDeck deck = await DrawFromDeck.SelectPowerCardDeck( this );
		bool forget = (deck.PowerType == PowerType.Major) // is major
			&& (forgetCardForMajor == null || await forgetCardForMajor( deck ));
		return await DrawInner(deck, 4, 1, forget );
	}

	public Task<DrawCardResult> DrawMinor( int numberToDraw=4, int numberToKeep=1 )
		=> DrawInner( GameState.Current.MinorCards, numberToDraw, numberToKeep, false );

	public Task<DrawCardResult> DrawMajor( bool forgetCard = true, int numberToDraw=4, int numberToKeep=1 )
		=> DrawInner( GameState.Current.MajorCards, numberToDraw, numberToKeep, forgetCard );

	protected virtual async Task<DrawCardResult> DrawInner( PowerCardDeck deck, int numberToDraw, int numberToKeep, bool forgetACard ) {
		var card = await DrawFromDeck.DrawInner( this, deck, numberToDraw, numberToKeep );
		if(forgetACard)
			await this.ForgetACard();
		return card;
	}

	#endregion

	#region Play Cards


	/// <summary> Plays card from hand for cost. </summary>
	public async Task SelectAndPlayCardsFromHand( int? numberToPlay = null )
		=> await SelectAndPlayCardsFromHand_Inner( numberToPlay ?? NumberOfCardsPlayablePerTurn );

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
		var card = await this.SelectPowerCard( prompt, remainingToPlay, powerCardOptions, CardUse.Play, Present.Done );
		if(card == null) return false;
		PlayCard( card );
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
		AddElements( card );

		AddActionFactory( card );
	}

	protected void AddElements( PowerCard card ) {
		foreach(var el in card.Elements)
			Elements.Add(el.Key,el.Value);
	}

	#endregion

	#region Save/Load Memento

	object IHaveMemento.Memento {
		get => new Memento(this);
		set => ((Memento)value).Restore(this);
	}

	// Whatever this returns, get saved to the memento
	protected virtual object CustomMementoValue {
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
		readonly object _tag = _spirit.CustomMementoValue;
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

	#region Tarteting / Range

	/// <summary> 
	/// Calculates Source for *TARGETING* *Powers* only.  
	/// Don't use it for non-power calculations.
	/// Don't use it for non-targeting Range-only calculations.
	/// </summary>
	public ITargetingSourceStrategy TargetingSourceStrategy = new DefaultPowerSourceStrategy();

	/// <summary> Calculates the Range for *Powers* only.  Don't use it for non-power calculations. </summary>
	public ICalcRange PowerRangeCalc = new DefaultRangeCalculator();

	/// <summary> Used EXCLUSIVELY For Targeting a PowerCard's Space </summary>
	/// <remarks> This used as the hook for Shadow's Pay-1-to-target-land-with-dahan </remarks>
	public virtual async Task<Space> TargetsSpace( 
		string prompt,
		IPreselect preselect,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria
	) {
		Space[] spaces = GetPowerTargetOptions( GameState.Current, sourceCriteria, targetCriteria ).ToArray();
		
		if(spaces.Length == 0) {
			ActionScope.Current.LogDebug($"{prompt} => No elligible spaces found!"); // show in debug window why nothing happened.
			return null;
		}

		var mySpace = preselect != null && UserGateway.UsePreselect.Value
			? await preselect.PreSelect(this, spaces)
			: await SelectAsync(new A.SpaceDecision(prompt, spaces, Present.Always));

		return mySpace;
	}

	public IEnumerable<Space> FindTargettingSourcesFor( SpaceSpec target, TargetingSourceCriteria sourceCriteria, params TargetCriteria[] targetCriteria ) {
		return sourceCriteria.GetSources(this)
			.Where(source => IsOriginFor(source,target,targetCriteria))
			.ToArray();
	}

	/// <summary> Determines if target can be reached from the specified source given any of the TargetCriteria </summary>
	/// <remarks>Used to find Origin lands</remarks>
	public bool IsOriginFor( Space source, SpaceSpec target, params TargetCriteria[] targetCriteria ) {
		var targetSpace = target.ScopeSpace;
		return targetCriteria.Any( tc => 
			PowerRangeCalc.GetSpaceOptions( source, tc ).Contains( targetSpace )
		);
	}

	// Helper for calling SourceCalc & RangeCalc, only for POWERS
	protected virtual IEnumerable<Space> GetPowerTargetOptions(
		GameState gameState,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria // allows different criteria at different ranges
	) {	
		IEnumerable<Space> sources = sourceCriteria.GetSources(this);

		// Convert TargetCriteria to spaces and merge (distinct) them together.
		return PowerRangeCalc.GetSpaceOptions( sources, targetCriteria )
			.ToArray();
	}

	// Non-targetting, For Power, Range-From Presence finder
	public IEnumerable<Space> FindSpacesWithinRange( TargetCriteria targetCriteria ) {
		ICalcRange rangeCalculator = ActionIsMyPower ? PowerRangeCalc : DefaultRangeCalculator.Singleton;
		return rangeCalculator
			.GetSpaceOptions( Presence.Lands, targetCriteria );
	}

	#endregion

	// Works like badlands.
	public int BonusDamage { get; set; } // This is a hack for Flame's Fury

	Spirit IHaveASpirit.Self => this;

}


public class SpiritDeck {
	/// <remarks> Unused at present. anticipated future use.</remarks>
	public DeckType Type { get; set; }
	public List<PowerCard> Cards;
	public enum DeckType { Hand, InPlay, Discard, DaysThatNeverWere_Major, DaysThatNeverWere_Minor, Other  };
}

public interface IHaveSecondaryElements {
	CountDictionary<Element> SecondaryElements { get; }
}

public enum ECouldHaveElements { No, Yes, AsPrepared }