namespace SpiritIsland;

public abstract partial class Spirit : IOption {

	#region constructor

	public Spirit( SpiritPresence presence, params PowerCard[] initialCards ){
		Presence = presence;
		Presence.SetSpirit( this );
		Presence.TrackRevealed.Add( args => Elements.AddRange(args.Track.Elements) );

		foreach(var card in initialCards)
			AddCardToHand(card);

		_gateway = new UserGateway();

		decks.Add(new SpiritDeck{ Icon = Img.Deck_Hand, Cards = Hand });
		decks.Add(new SpiritDeck{ Icon = Img.Deck_Played, Cards = InPlay });
		decks.Add(new SpiritDeck{ Icon = Img.Deck_Discarded, Cards = DiscardPile } );
	}

	public void AddCardToHand( PowerCard card ){
		Hand.Add(card);
	}

	#endregion

	#region Gateway stuff

	public IUserPortal Portal => _gateway;
	public Task<T> Select<T>( A.TypedDecision<T> decision ) where T : class, IOption => _gateway.Select<T>( decision );
	public void PreSelect( SpaceToken st ) => _gateway.Preloaded = st;
	UserGateway _gateway;

	#endregion

	#region Elements

	public readonly ElementCounts Elements = new ElementCounts();

	/// <summary>
	/// Checks all elements that are available to spirit.
	/// </summary>
	public virtual bool CouldHaveElements( ElementCounts subset ) {
		// For normal spirits without Prepared Elements, this is only the normal Elements
		int wildCount = Elements[Element.Any];
		return wildCount == 0 ? Elements.Contains(subset)  // no 'wild-card' elements, Elements must contain subset
			: subset.Except(Elements).Count <= wildCount; // Find missing elements and count if they are less than our 'wild-card' elements
	}

	/// <summary>
	/// Checks elements available, and commits them (like the 'Any' element)
	/// </summary>
	public virtual async Task<bool> HasElements( ElementCounts subset ) {
		// For normal spirits without Prepared Elements, this is the same as Could Have Elements
		if( Elements.Contains(subset) ) return true;
		int wildCount = Elements[Element.Any];
		if(wildCount == 0) return false;

		// We have some wild cards
		var missing = subset.Except( Elements );
		if(missing.Count > wildCount) return false;

		if( await this.UserSelectsFirstText("Activate: "+subset.BuildElementString()+"?", $"Yes, use {missing.Count} 'Any' elments", "No thanks" )) {
			foreach(var p in missing) Elements[p.Key] += p.Value;
			Elements[Element.Any] -= missing.Count;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Spirit determines which Option they can activate.
	/// </summary>
	/// <remarks>overriden by Shiftin Memories and Volcano</remarks>
	public virtual async Task<IDrawableInnateTier> SelectInnateTierToActivate( IEnumerable<IDrawableInnateTier> innateOptions ) {
		IDrawableInnateTier match = null;
		foreach(var option in innateOptions.OrderBy( o => o.Elements.Total ))
			if(await HasElements( option.Elements ))
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
		await using ActionScope action2 = await ActionScope.Start( ActionCategory.Spirit_PresenceTrackIcon );
		await ApplyRevealedPresenceTracks_Inner( BindSelf() );
	}

	protected virtual async Task ApplyRevealedPresenceTracks_Inner( SelfCtx ctx ) {

		// Energy
		Energy += EnergyPerTurn;
		await EnergyCollected.InvokeAsync(this);
		// ! Elements were added when the round started.

		// Do actions AFTER energy and elements have been added - in case playing ManyMindsMoveAsOne - Pay 2 for power card.
		foreach(var action in Presence.RevealedActions)
			await action.ActAsync( ctx );

	}


	public AsyncEvent<Spirit> EnergyCollected = new AsyncEvent<Spirit>();

	public async Task ResolveActions( GameState gs ) {
		Phase phase = gs.Phase;

		while(GetAvailableActions( phase ).Any()
			&& await ResolveAction( phase )
		) { }

	}

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

	public List<PowerCard> Hand = new List<PowerCard>();	    // in hand
	public List<PowerCard> InPlay = new List<PowerCard>();		// paid for / played
	public List<PowerCard> DiscardPile = new List<PowerCard>(); // Cards are not transferred to discard pile until end of turn because we need to keep track of their elements.

	public SpiritDeck[] Decks => decks.ToArray();

	// This is not part of Decks because these cards are transient and don't actually belong to the spirit.
	public List<PowerCard> DraftDeck = new List<PowerCard>();   // The virtual deck the cards are in while they are being drawn/drafted

	readonly protected List<SpiritDeck> decks = new List<SpiritDeck>();

	readonly List<IActionFactory> _availableActions = new List<IActionFactory>();
	readonly HashSet<IActionFactory> _usedActions = new HashSet<IActionFactory>();
	readonly List<InnatePower>       _usedInnates = new List<InnatePower>();

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
		if(reclaimCard == null ) throw new ArgumentNullException(nameof(reclaimCard));
		if( DiscardPile.Contains( reclaimCard ) ) {
			DiscardPile.Remove( reclaimCard );
		} else if( InPlay.Contains( reclaimCard ) ) {
			RemoveCardFromPlay( reclaimCard );
		} else
			throw new InvalidOperationException("Can't find the card to reclaim. Not in dicard nor in play");
		Hand.Add( reclaimCard );
	}

	public async Task Reclaim1FromDiscard() {
		PowerCard cardToReclaim = await this.SelectPowerCard( "Select card to reclaim.", DiscardPile, CardUse.Reclaim, Present.Always );
		if( cardToReclaim != null )
			Reclaim( cardToReclaim );
	}

	/// <summary>
	/// Called mid-round.  If reclaiming card from hand, looses elements
	/// </summary>
	public async Task Reclaim1FromDiscardOrPlayed() {
		PowerCard cardToReclaim = await this.SelectPowerCard( "Select card to reclaim.", DiscardPile.Union(InPlay), CardUse.Reclaim, Present.Always );
		Reclaim( cardToReclaim );
	}

	/// <summary> Forget USER SELECTED power card. </summary>
	public virtual async Task<PowerCard> ForgetACard( IEnumerable<PowerCard> options = null, Present present = Present.Always ) {

		options ??= GetForgetableCards();

		PowerCard cardToForget = await this.SelectPowerCard( "Select power card to forget", options, CardUse.Forget, present );
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
			Elements[el.Key] -= el.Value;// lose elements from forgotten card
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
			throw new InvalidOperationException( $"Unable to remove ActionFactory {selectedActionFactory.Name} from Unresolved Actions because it is not there." );
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
	}

	public virtual async Task TakeActionAsync(IActionFactory factory, Phase phase) {
		await using ActionScope actionScope = await CreateActionScopeForSpiritAction( phase );
		SelfCtx ctx = Bind( phase );
		RemoveFromUnresolvedActions( factory ); // removing first, so action can restore it if desired
		await factory.ActivateAsync( ctx );
		GameState.Current.CheckWinLoss(); // @@@
	}

	#endregion

	#region Spin-Up ActionScope for spirit action

	protected SelfCtx Bind( Phase phase ) {
		return phase switch {
			Phase.Init or
			Phase.Growth => BindSelf(),
			Phase.Fast or
			Phase.Slow => BindMyPowers(),
			_ => throw new InvalidOperationException(),
		};
	}

	protected async Task<ActionScope> CreateActionScopeForSpiritAction( Phase phase ) {
		ActionScope actionScope1 = await ActionScope.Start( GetActionCategoryForSpiritAction( phase ) );
		actionScope1.Owner = this;
		return actionScope1;
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

	/// <summary> Energy gain per turn </summary>
	public int EnergyPerTurn => Presence.EnergyPerTurn;
	public virtual int NumberOfCardsPlayablePerTurn => Presence.CardPlayPerTurn + tempCardPlayBoost;

	public int tempCardPlayBoost = 0;


	#endregion

	public abstract string Text { get; }

	abstract public SpecialRule[] SpecialRules { get; }

	public virtual InnatePower[] InnatePowers { get; set; } = Array.Empty<InnatePower>();

	public void InitSpirit( Board board, GameState gameState ){
		gameState.TimePasses_WholeGame += On_TimePassed;
		_gateway.DecisionMade += (d) => gameState.Log(d);
		InitializeInternal(board,gameState);
	}

	protected abstract void InitializeInternal( Board board, GameState gameState );

	#region Bind helpers

	public SelfCtx BindSelf() => BindDefault( this );

	public SelfCtx BindMyPowers() => BindMyPowers( this );

	public bool ActionIsMyPower {
		get {
			var scope = ActionScope.Current;
			return scope.Category == ActionCategory.Spirit_Power 
				&& scope.Owner == this;
		}
	}

	#endregion Bind helpers

	public virtual SelfCtx BindDefault( Spirit spirit )	=> new SelfCtx( spirit );

	public virtual SelfCtx BindMyPowers( Spirit spirit ) => new SelfCtx( spirit );

	Task On_TimePassed(GameState _ ) {
		// reset cards / powers
		DiscardPile.AddRange( InPlay );
		InPlay.Clear();
		_availableActions.Clear();
		_usedActions.Clear();
		_usedInnates.Clear();

		// Card plays
		tempCardPlayBoost = 0;

		// Elements
		InitElementsFromPresence();

		return Task.CompletedTask;
	}

	public void InitElementsFromPresence() {
		Elements.Clear();
		Elements.Add( Presence.TrackElements );
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

	// Used by Flame's Fury to detect new actions
	// Used by Observe The Ever-Changing World to distinguish between actions

	#region Play Cards

	// Plays cards from hand for cost
	/// <summary>
	/// Plays card from hand for cost.
	/// </summary>
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
		var card = await this.SelectPowerCard( prompt, powerCardOptions, CardUse.Play, Present.Done );
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
			Elements[el.Key] += el.Value;
	}

	#endregion

	#region Save/Load Memento

	public IMemento<Spirit> SaveToMemento() => new Memento(this);

	public void LoadFrom( IMemento<Spirit> memento ) => ((Memento)memento).Restore(this);


	// Whatever this returns, get saved to the memento
	protected virtual object _customSaveValue {
		get { return null; }
		set { }
	}

	protected class Memento : IMemento<Spirit> {
		public Memento(Spirit spirit) {
			energy = spirit.Energy;
			bonusCardPlay = spirit.tempCardPlayBoost;
			elements = spirit.Elements.ToArray();
			// preparedElements = spirit.PreparedElements.ToArray();
			presence = spirit.Presence.SaveToMemento();
			hand      = spirit.Hand.ToArray();
			purchased = spirit.InPlay.ToArray();
			discarded = spirit.DiscardPile.ToArray();
			available = spirit._availableActions.ToArray();
			usedActions = spirit._usedActions.ToArray();
			usedInnates = spirit._usedInnates.ToArray();
			energyCollected = spirit.EnergyCollected.SaveToMemento();
			tag = spirit._customSaveValue;
		}
		public void Restore(Spirit spirit) {
			spirit.Energy = energy;
			spirit.tempCardPlayBoost = bonusCardPlay;
			InitFromArray( spirit.Elements, elements);
			spirit.Presence.LoadFrom(presence);
			spirit.Hand.SetItems( hand );
			spirit.InPlay.SetItems( purchased );
			spirit.DiscardPile.SetItems( discarded );
			spirit._availableActions.SetItems( available );
			spirit._usedActions.SetItems( usedActions );
			spirit._usedInnates.SetItems( usedInnates );
			spirit.InitElementsFromPresence();
			spirit.BonusDamage = 0; // assuming beginning of round
			spirit.EnergyCollected.LoadFrom( energyCollected );
			spirit._customSaveValue = tag;
		}
		readonly int energy;
		readonly int bonusCardPlay;
		readonly KeyValuePair<Element,int>[] elements;
		readonly IMemento<SpiritPresence> presence;
		readonly PowerCard[] hand;
		readonly PowerCard[] purchased;
		readonly PowerCard[] discarded;
		readonly IActionFactory[] available;
		readonly IActionFactory[] usedActions;
		readonly InnatePower[] usedInnates;
		readonly IMemento<AsyncEvent<Spirit>> energyCollected;
		readonly object tag;
	}
	static public void InitFromArray( ElementCounts dict, KeyValuePair<Element, int>[] array ) {
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
		SelfCtx ctx, // this has the new Action for this action.
		string prompt,
		IPreselect preselect,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria
	) {
		SpaceState[] spaces = GetPowerTargetOptions( GameState.Current, sourceCriteria, targetCriteria ).ToArray();
		
		if(spaces.Length == 0) {
			GameState.Current.LogDebug($"{prompt} => No elligible spaces found!"); // show in debug window why nothing happened.
			return null;
		}

		return preselect != null && UserGateway.UsePreselect.Value
			? await preselect.PreSelect( ctx.Self, spaces )
			: await Select( new A.Space( prompt, spaces.Downgrade(), Present.Always ));
	}

	public IEnumerable<SpaceState> FindTargettingSourcesFor( Space target, TargetingSourceCriteria sourceCriteria, params TargetCriteria[] targetCriteria ) {
		return sourceCriteria.GetSources(this)
			.Where(source => IsOriginFor(source,target,targetCriteria))
			.ToArray();
	}

	/// <summary> Determines if target can be reached from the specified source given any of the TargetCriteria </summary>
	/// <remarks>Used to find Origin lands</remarks>
	public bool IsOriginFor( SpaceState source, Space target, params TargetCriteria[] targetCriteria ) {
		return targetCriteria.Any( tc => 
			PowerRangeCalc.GetSpaceOptions( source, tc ).Contains( target )
		);
	}

	// Helper for calling SourceCalc & RangeCalc, only for POWERS
	protected virtual IEnumerable<SpaceState> GetPowerTargetOptions(
		GameState gameState,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria // allows different criteria at different ranges
	) {	
		IEnumerable<SpaceState> sources = sourceCriteria.GetSources(this);

		// Convert TargetCriteria to spaces and merge (distinct) them together.
		return PowerRangeCalc.GetSpaceOptions( sources, targetCriteria )
			.ToArray();
	}

	// Non-targetting, For Power, Range-From Presence finder
	public IEnumerable<SpaceState> FindSpacesWithinRange( TargetCriteria targetCriteria ) {
		ICalcRange rangeCalculator = ActionIsMyPower ? PowerRangeCalc : DefaultRangeCalculator.Singleton;
		return rangeCalculator
			.GetSpaceOptions( Presence.Spaces.Tokens(), targetCriteria );
	}

	#endregion

	// Works like badlands.
	public int BonusDamage { get; set; } // This is a hack for Flame's Fury


	/// <param name="groups">Option: if null/empty, no filtering</param>
	public virtual async Task AddStrife( SpaceState tokens, params HumanTokenClass[] groups ) {
		var st = await Select( An.Invader.ForStrife( tokens, groups ) );
		if(st == null) return;
		await tokens.Add1StrifeTo( st.Token.AsHuman() );
	}

}


public class SpiritDeck {
	public Img Icon;
	public List<PowerCard> Cards;
}

public interface IHaveSecondaryElements {
	ElementCounts SecondaryElements { get; }
}