namespace SpiritIsland;

public abstract partial class Spirit : IOption {

	#region constructor

	public Spirit( SpiritPresence presence, params PowerCard[] initialCards ){
		Presence = presence;
		Presence.SetSpirit( this );
		Presence.TrackRevealed.ForGame.Add( args => Elements.AddRange(args.Track.Elements) );

		foreach(var card in initialCards)
			AddCardToHand(card);

		Gateway = new UserGateway();

		decks.Add(new SpiritDeck{ Icon = Img.Deck_Hand, PowerCards = Hand });
		decks.Add(new SpiritDeck{ Icon = Img.Deck_Played, PowerCards = InPlay });
		decks.Add(new SpiritDeck{ Icon = Img.Deck_Discarded, PowerCards = DiscardPile } );
	}

	public void AddCardToHand( PowerCard card ){
		Hand.Add(card);
	}

	public UserGateway Gateway { get; }

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

	public virtual async Task<IDrawableInnateOption> SelectInnateToActivate( IEnumerable<IDrawableInnateOption> innateOptions ) {
		IDrawableInnateOption match = null;
		foreach(var option in innateOptions.OrderBy( o => o.Elements.Total ))
			if(await HasElements( option.Elements ))
				match = option;
		return match;
	}

	#endregion

	#region Growth

	public GrowthTrack GrowthTrack { get; protected set; }

	public async Task DoGrowth(GameState gameState) {
		await new DoGrowthClass(this,gameState).Execute();
	}

	public async Task GrowAndResolve( GrowthOption option, GameState gameState ) { // public for Testing
		await using var action = new ActionScope( ActionCategory.Spirit_Growth );
		var ctx = BindSelf();

		// Auto run the auto-runs.
		foreach(var autoAction in option.AutoRuns)
			await autoAction.ActivateAsync( ctx );

		// If Option has only 1 Action, auto trigger it.
		if( option.UserRuns.Count() == 1) {
			await option.UserRuns.First().ActivateAsync( ctx );
		} else {
			QueueUpGrowth( option );
			await ResolveActions( gameState );
		}
	}

	/// <summary> Adds UserRun Growth Actions to the Ready-to-resolve list.</summary>
	public void QueueUpGrowth( GrowthOption option ) { // Public for Testing
		foreach(GrowthActionFactory action in option.UserRuns)
			AddActionFactory( action );
	}

	protected async Task ApplyRevealedPresenceTracks( SelfCtx ctx ) {

		// Energy
		Energy += EnergyPerTurn;
		await EnergyCollected.InvokeAsync(this);
		// ! Elements were added when the round started.

		// Do actions AFTER energy and elements have been added - in case playing ManyMindsMoveAsOne - Pay 2 for power card.
		foreach(IActionFactory action in Presence.RevealedActions.Cast<IActionFactory>())
			await action.ActivateAsync( ctx );

	}
	public DualAsyncEvent<Spirit> EnergyCollected = new DualAsyncEvent<Spirit>();


	public async Task ResolveActions( GameState gs ) {  // !!! Seems like this should be private / protected and not called from outside.
		Phase phase = gs.Phase;

		while( GetAvailableActions( phase ).Any()
			&& await ResolveAction( phase, gs )
		) {}

	}

	public async Task<bool> ResolveAction( Phase phase, GameState gs ) {
		Present present = phase == Phase.Growth ? Present.Always : Present.Done;

		var category = phase switch {
			Phase.Init or
			Phase.Growth => ActionCategory.Spirit_Growth,
			Phase.Fast or
			Phase.Slow => ActionCategory.Spirit_Power,
			_ => throw new InvalidOperationException(),
		};

		await using var actionScope = new ActionScope( category );
		actionScope.Owner = this;

		SelfCtx ctx = phase switch {
			Phase.Init or
			Phase.Growth => BindSelf(),
			Phase.Fast or 
			Phase.Slow => BindMyPowers( gs ),
			_ => throw new InvalidOperationException(),
		};

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

		await TakeAction( option, ctx );
		return true;
	}


	#endregion

	#region Cards

	public List<PowerCard> Hand = new List<PowerCard>();	    // in hand
	public List<PowerCard> InPlay = new List<PowerCard>();		// paid for / played
	public List<PowerCard> DiscardPile = new List<PowerCard>(); // Cards are not transferred to discard pile until end of turn because we need to keep track of their elements.
	public SpiritDeck[] Decks => decks.ToArray();

	readonly protected List<SpiritDeck> decks = new List<SpiritDeck>();

	readonly List<IActionFactory> availableActions = new List<IActionFactory>();
	readonly HashSet<IActionFactory> usedActions = new HashSet<IActionFactory>();
	readonly List<InnatePower>       usedInnates = new List<InnatePower>();

	// so spirits can replay used cards or collect them instead of discard
	public IEnumerable<IActionFactory> UsedActions => usedActions;

	public virtual IEnumerable<IActionFactory> GetAvailableActions(Phase speed) {
		foreach(var action in AvailableActions) {
			if( IsActiveDuring( speed, action ) )
				yield return action;
		}
	}

	// Holds Fast and Slow actions,
	// depends on Fast/Slow phase to only select the actions that are appropriate
	protected IEnumerable<IActionFactory> AvailableActions { 
		get {
			foreach(IActionFactory action in availableActions)
				yield return action;

			foreach(InnatePower innate in InnatePowers)
				if( !usedInnates.Contains( innate ) )
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
	public virtual async Task<PowerCard> ForgetOne( IEnumerable<PowerCard> restrictedOptions = null, Present present = Present.Always ) {
		var options = restrictedOptions 
			?? InPlay					// in play
				.Union( Hand )          // in Hand
				.Union( DiscardPile )   // in Discard
				.ToArray();

		PowerCard cardToForget = await this.SelectPowerCard( "Select power card to forget", options, CardUse.Forget, present );
		if( cardToForget != null )
			Forget( cardToForget );
		return cardToForget;
	}

	public virtual void Forget( PowerCard cardToRemove ) {
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

	public bool IsActiveDuring(Phase phase, IActionFactory actionFactory) => actionFactory.CouldActivateDuring( phase, this );

	/// <summary>
	/// Removes it from the Unresolved-list
	/// </summary>
	public virtual void RemoveFromUnresolvedActions(IActionFactory selectedActionFactory ) {
		if(selectedActionFactory is InnatePower ip) { // Could reverse this and instead of listing the used, create a not-used list that we remove them from when used
			usedInnates.Add( ip );
			return;
		}

		int index = availableActions.IndexOf( selectedActionFactory );
		if(index == -1) 
			throw new InvalidOperationException( "can't remove factory that isn't there." );
		usedActions.Add(availableActions[index]);
		availableActions.RemoveAt( index );

	}

	public void AddActionFactory( IActionFactory factory ) {
		availableActions.Add( factory );
	}

	public virtual async Task TakeAction(IActionFactory factory, SelfCtx ctx) {
		RemoveFromUnresolvedActions( factory ); // removing first, so action can restore it if desired
		await factory.ActivateAsync( ctx );
		if(factory is IRecordLastTarget lastTargetRecorder )
			await ActionTaken_ThisRound.InvokeAsync( new ActionTaken(factory,lastTargetRecorder.LastTarget) );
		ctx.GameState.CheckWinLoss(); // @@@
	}

	public AsyncEvent<ActionTaken> ActionTaken_ThisRound = new AsyncEvent<ActionTaken>();

	#endregion

	#region presence

	public SpiritPresenceToken Token => Presence.Token; // helper shortcut

	/// <summary> # of coins in the bank. </summary>
	public int Energy { 
		get => _energy;
		set => _energy = value > 0 ? value : 0; // Be safe - Erosion of Fear tries to make this negative.
	}
	int _energy;

	public SpiritPresence Presence {get;}

	/// <summary> Energy gain per turn </summary>
	public int EnergyPerTurn => Presence.EnergyPerTurn;
	public virtual int NumberOfCardsPlayablePerTurn => Presence.CardPlayCount + tempCardPlayBoost;

	public int tempCardPlayBoost = 0;


	#endregion

	public abstract string Text { get; }

	abstract public SpecialRule[] SpecialRules { get; }

	public virtual InnatePower[] InnatePowers { get; set; } = Array.Empty<InnatePower>();

	public void InitSpirit( Board board, GameState gameState ){
		gameState.TimePasses_WholeGame += On_TimePassed;
		Gateway.DecisionMade += (d) => gameState.Log(d);
		InitializeInternal(board,gameState);
	}

	protected abstract void InitializeInternal( Board board, GameState gameState );

	#region Bind helpers
	public SelfCtx BindSelf() => BindDefault( this );
	public SelfCtx BindMyPowers( GameState gameState ) => BindMyPowers( this, gameState );
	#endregion Bind helpers

	public virtual SelfCtx BindDefault( Spirit spirit )	=> new SelfCtx( spirit );

	public virtual SelfCtx BindMyPowers( Spirit spirit, GameState gameState ) => new SelfCtx( spirit );

	Task On_TimePassed(GameState _ ) {
		// reset cards / powers
		DiscardPile.AddRange( InPlay );
		InPlay.Clear();
		availableActions.Clear();
		usedActions.Clear();
		usedInnates.Clear();

		// Card plays
		tempCardPlayBoost = 0;

		// Elements
		InitElementsFromPresence();

		ActionTaken_ThisRound.Clear();
		return Task.CompletedTask;
	}

	public void InitElementsFromPresence() {
		Elements.Clear();
		Elements.Add( Presence.TrackElements );
	}

	// pluggable, draw power card
	#region Draw Card

	public virtual async Task<DrawCardResult> Draw( GameState gameState ) {

		PowerType powerType = await DrawFromDeck.SelectPowerCardType( this );
		DrawCardResult result = powerType == PowerType.Minor
			? await DrawFromDeck.DrawInner( this, gameState.MinorCards, 4, 1 )
			: await DrawFromDeck.DrawInner( this, gameState.MajorCards, 4, 1 );

		if (result.PowerType == PowerType.Major )
			await this.ForgetOne();
		return result;
	}

	public virtual Task<DrawCardResult> DrawMinor( GameState gameState, int numberToDraw=4, int numberToKeep=1 ) 
		=> DrawFromDeck.DrawInner( this, gameState.MinorCards, numberToDraw, numberToKeep );

	public virtual async Task<DrawCardResult> DrawMajor( GameState gameState, bool forgetCard = true, int numberToDraw=4, int numberToKeep=1 ) {
		var result = await DrawFromDeck.DrawInner( this, gameState.MajorCards, numberToDraw, numberToKeep );
		if(forgetCard)
			await this.ForgetOne();
		return result;
	}

	#endregion

	// Used by Flame's Fury to detect new actions
	// Used by Observe The Ever-Changing World to distinguish between actions

	#region Play Cards

	// Plays cards from hand for cost
	public async Task SelectAndPlayCardsFromHand( int? numberToPlay = null ) {
		int remainingToPlay = numberToPlay ?? NumberOfCardsPlayablePerTurn;

		PowerCard[] getPowerCardOptions() => Hand
			.Where( c => c.Cost <= Energy )
			.ToArray();

		// !!! there is a bug somewhere that allows duplicate cards in this list and crashes
		//  (maybe from Unlock the Gates of Deepest Power)
		var debug = new HashSet<PowerCard>();
		foreach(PowerCard d in getPowerCardOptions())
			if(debug.Contains(d)) throw new Exception($"Card {d.Name} found twice"); else debug.Add(d);

		PowerCard[] powerCardOptions;
		while(0 < remainingToPlay
			&& 0 < (powerCardOptions = getPowerCardOptions()).Length
		) {
			string prompt = $"Play power card (${Energy} / {remainingToPlay})";
			var card = await this.SelectPowerCard( prompt, powerCardOptions, CardUse.Play, Present.Done );
			if(card != null) {
				PlayCard( card );
				--remainingToPlay;
			} else
				remainingToPlay = 0;

		}
	}

	public void PlayCard( PowerCard card, int? cost = null ) {
		if(!cost.HasValue) cost = card.Cost;

		if(!Hand.Contains( card )) throw new CardNotAvailableException();
		if(Energy < cost) throw new InsufficientEnergyException();

		Hand.Remove( card );
		InPlay.Add( card );
		Energy -= cost.Value;
		foreach(var el in card.Elements )
			Elements[el.Key] += el.Value;

		AddActionFactory( card );
	}

	#endregion

	#region Save/Load Memento
	public virtual IMemento<Spirit> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<Spirit> memento ) => ((Memento)memento).Restore(this);

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
			available = spirit.availableActions.ToArray();
			usedActions = spirit.usedActions.ToArray();
			usedInnates = spirit.usedInnates.ToArray();
			energyCollected = spirit.EnergyCollected.SaveToMemento();
		}
		public void Restore(Spirit spirit) {
			spirit.Energy = energy;
			spirit.tempCardPlayBoost = bonusCardPlay;
			InitFromArray( spirit.Elements, elements);
			spirit.Presence.LoadFrom(presence);
			spirit.Hand.SetItems( hand );
			spirit.InPlay.SetItems( purchased );
			spirit.DiscardPile.SetItems( discarded );
			spirit.availableActions.SetItems( available );
			spirit.usedActions.SetItems( usedActions );
			spirit.usedInnates.SetItems( usedInnates );
			spirit.InitElementsFromPresence();
			spirit.BonusDamage = 0; // assuming beginning of round
			spirit.EnergyCollected.LoadFrom( energyCollected );
		}
		static public void InitFromArray(ElementCounts dict, KeyValuePair<Element,int>[] array ) {
			dict.Clear(); 
			foreach(var p in array) dict[p.Key]=p.Value;
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
		readonly IMemento<DualAsyncEvent<Spirit>> energyCollected;
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
	// !!! make this private so nothing tries to use it.  All targetting should go through Spirit methods
	public ICalcPowerTargetingSource TargetingSourceCalc = new DefaultPowerSourceCalculator();
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
		prompt ??= "Target Space.";
		IEnumerable<SpaceState> spaces = GetPowerTargetOptions( ctx.GameState, sourceCriteria, targetCriteria );

		if(preselect != null) {
			var spaceTokenOptions = spaces
				.SelectMany( ss=>ss.SpaceTokensOfAnyClass(preselect.TokenClasses) )
				.ToArray();

			SpaceToken st = await ctx.Self.Gateway.Decision( new Select.TokenFromManySpaces( preselect.Prompt, spaceTokenOptions, Present.Always ) );
			if(st == null) return null;
			Gateway.Preloaded = st;
			return st.Space;
		}

		return await this.Gateway.Decision( new Select.ASpace( prompt, spaces.Downgrade(), Present.Always ));
	}

	// Helper for calling SourceCalc & RangeCalc, only for POWERS
	protected virtual IEnumerable<SpaceState> GetPowerTargetOptions(
		GameState gameState,
		TargetingSourceCriteria sourceCriteria,
		params TargetCriteria[] targetCriteria // allows different criteria at different ranges
	) {	
		// Converts SourceCriteria to Spaces
		IEnumerable<SpaceState> sources = TargetingSourceCalc.FindSources( 
			this.Presence,
			sourceCriteria,
			gameState		// needed only for Entwined power - to get the other spirit's location
		).ToArray();

		// Convert TargetCriteria to spaces and merge (distinct) them together.
		var debugResults = targetCriteria
			.SelectMany(tc => PowerRangeCalc.GetTargetOptionsFromKnownSource( sources, tc ))
			.Distinct()
			.ToArray();

		return debugResults;
	}

	// Non-targetting, For Power, Range-From Presence finder
	public IEnumerable<SpaceState> FindSpacesWithinRange( TargetCriteria targetCriteria, bool forPower ) {
		return (forPower ? PowerRangeCalc : DefaultRangeCalculator.Singleton)
			.GetTargetOptionsFromKnownSource( Presence.ActiveSpaceStates, targetCriteria );
	}

	#endregion

	// Works like badlands.
	public int BonusDamage { get; set; } // This is a hack for Flame's Fury

}


public class SpiritDeck {
	public Img Icon;
	public List<PowerCard> PowerCards;
}

public interface IHaveSecondaryElements {
	ElementCounts SecondaryElements { get; }
}