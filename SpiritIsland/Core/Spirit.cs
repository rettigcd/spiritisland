using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class Spirit : IOption {

		#region constructor

		public Spirit( SpiritPresence presence, params PowerCard[] initialCards ){
			Presence = presence;
			Presence.TrackRevealed.ForGame.Add( Presence_TrackRevealed );

			foreach(var card in initialCards)
				AddCardToHand(card);

			Action = new ActionGateway();
		}

		public void AddCardToHand( PowerCard card ){
			Hand.Add(card);
		}

		public ActionGateway Action { get; }

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

		public virtual async Task<ElementCounts> GetHighestMatchingElements( IEnumerable<ElementCounts> elementOptions ) {
			ElementCounts match = null;
			foreach(ElementCounts elements in elementOptions.OrderBy( els => els.Total ))
				if(await HasElements( elements ))
					match = elements;
			return match;
		}

		#endregion

		#region Growth

		public GrowthOptionGroup Growth { get; protected set; }

		public virtual async Task DoGrowth(GameState gameState) {
			var ctx = new SpiritGameStateCtx( this, gameState, Cause.Growth );

			// (a) Resolve Initialization
			if(availableActions.Any()) {
				if(availableActions.Count == 1) 
					await TakeAction( availableActions[0], ctx );
				else
					await ResolveActions(ctx);
			}

			// (b) Pre Growth Track options
			foreach(ITrackActionFactory action in Presence.RevealedActions)
				if( !action.RunAfterGrowthResult )
					await action.ActivateAsync( ctx );

			// (c) Growth
			int count = Growth.SelectionCount;
			List<GrowthOption> remainingOptions = Growth.Options.ToList();

			while(count-- > 0) {
				var currentOptions = remainingOptions.Where( o => o.GainEnergy + Energy >= 0 ).ToArray();
				GrowthOption option = (GrowthOption)await this.Select( "Select Growth Option", currentOptions, Present.Always );
				remainingOptions.Remove( option );

				await GrowAndResolve( option, gameState );
			}

			// (d) Post Growth Track options
			await ApplyRevealedPresenceTracks( ctx );

		}

		public Task GrowAndResolve( GrowthOption option, GameState gameState ) {
			var ctx = new SpiritGameStateCtx(this,gameState,Cause.Growth);

			if( option.GrowthActions.Length == 1 )
				return option.GrowthActions[0].ActivateAsync( ctx );
			
			Grow( option );
			return ResolveActions( ctx );

		}

		public void Grow( GrowthOption option ) {
			foreach(GrowthActionFactory action in option.GrowthActions)
				AddActionFactory( action );
		}

		async Task Presence_TrackRevealed( GameState gs, Track track ) {
			Elements.AddRange( track.Elements );

			if( track.Action != null && (gs.Phase != Phase.Growth || !track.Action.RunAfterGrowthResult) )
				await track.Action.ActivateAsync(new SpiritGameStateCtx(this,gs,Cause.Power));
		}

		public Task ApplyRevealedPresenceTracks(GameState gs) {
			var ctx = new SpiritGameStateCtx( this, gs, Cause.Growth );
			return this.ApplyRevealedPresenceTracks(ctx);
		}
		protected async Task ApplyRevealedPresenceTracks( SpiritGameStateCtx ctx ) {

			// Energy
			Energy += EnergyPerTurn;
			// Elements were added when the round started.


			// Do actions AFTER energy and elements have been added - in case playing ManyMindsMoveAsOne - Pay 2 for power card.
			foreach(ITrackActionFactory action in Presence.RevealedActions)
				if(action.RunAfterGrowthResult)
					await action.ActivateAsync( ctx );

		}

		// !!! Seems like this should be private / protected and not called from outside.
		public async Task ResolveActions( SpiritGameStateCtx ctx ) {
			Phase speed = ctx.GameState.Phase;
			Present present = ctx.GameState.Phase == Phase.Growth ? Present.Always : Present.Done;

			IActionFactory[] factoryOptions;

			while((factoryOptions = this.GetAvailableActions( speed ).ToArray()).Length> 0) {

				// -------------
				// Select Actions to resolve
				// -------------
				IActionFactory option = await this.SelectFactory( "Select " + speed + " to resolve:", factoryOptions, present );
				if(option == null)
					break;

				// if user clicked a slow card that was made fast, // slow card won't be in the options
				if(!factoryOptions.Contains( option ))
					// find the fast version of the slow card that was clicked
					option = factoryOptions.Cast<IActionFactory>()
						.First( factory => factory == option );

				if(!factoryOptions.Contains( option ))
					throw new Exception( "Dude! - You selected something that wasn't an option" );

				await TakeAction( option, ctx );
			}

		}

		#endregion

		#region Cards

		public List<PowerCard> Hand = new List<PowerCard>();	    // in hand
		public List<PowerCard> InPlay = new List<PowerCard>();		// paid for / played
		public List<PowerCard> DiscardPile = new List<PowerCard>(); // Cards are not transferred to discard pile until end of turn because we need to keep track of their elements.

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

		public virtual async Task ForgetPowerCard() {
			var options = InPlay.Union( Hand ).Union( DiscardPile )
				.ToArray();
			PowerCard cardToForget = await this.SelectPowerCard( "Select power card to forget", options, CardUse.Forget, Present.Always );
			Forget( (PowerCard)cardToForget );
		}

		public virtual void Forget( PowerCard cardToRemove ) {
			// A card can be in one of 3 places
			// (1) Purchased / Active
			if(InPlay.Contains( cardToRemove )) {
				foreach(var el in cardToRemove.Elements) 
					Elements[el.Key]-=el.Value;// lose elements from forgotten card
				InPlay.Remove( cardToRemove );
			}
			// (2) Unpurchased, still in hand
			Hand.Remove( cardToRemove );
			// (3) used, discarded
			DiscardPile.Remove( cardToRemove );
		}

		public bool IsActiveDuring(Phase phase, IActionFactory actionFactory) => actionFactory.CouldActivateDuring( phase, this );

		/// <summary>
		/// Removes it from the Unresolved-list
		/// </summary>
		public void RemoveFromUnresolvedActions(IActionFactory selectedActionFactory ) {
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

		public Spirit UsePowerProgression() {
			CardDrawer = GetPowerProgression();
			return this;
		}

		protected virtual async Task TakeAction(IActionFactory factory, SpiritGameStateCtx ctx) {
			var oldActionGuid = CurrentActionId; // capture old
			CurrentActionId = Guid.NewGuid(); // set new
			try {
				await factory.ActivateAsync( ctx );
				RemoveFromUnresolvedActions( factory );
			} finally {
				CurrentActionId = oldActionGuid; // restore
			}
			ctx.GameState.CheckWinLoss(); // @@@
		}

		protected virtual PowerProgression GetPowerProgression() => throw new NotImplementedException();

		#endregion

		#region presence

		/// <summary> # of coins in the bank. </summary>
		public int Energy { get; set; }

		public SpiritPresence Presence {get; }

		public virtual Task PlacePresence( IOption from, Space to, GameState gs ) {
			return Presence.Place(from, to, gs);
		}

		/// <summary> Energy gain per turn </summary>
		public int EnergyPerTurn => Presence.EnergyPerTurn;
		public virtual int NumberOfCardsPlayablePerTurn => Presence.CardPlayCount + tempCardPlayBoost;

		public int tempCardPlayBoost = 0;


		#endregion

		public abstract string Text { get; }

		abstract public SpecialRule[] SpecialRules { get; }

		public virtual InnatePower[] InnatePowers { get; set; } = Array.Empty<InnatePower>();

		public void Initialize( Board board, GameState gameState ){
			gameState.TimePasses_WholeGame += On_TimePassed;
			Action.DecisionMade += (d) => gameState.Log(d);
			InitializeInternal(board,gameState);
		}

		protected abstract void InitializeInternal( Board board, GameState gameState );

		void On_TimePassed(GameState _ ) {
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
		}

		public void InitElementsFromPresence() {
			Elements.Clear();
			Presence.AddElements( Elements );
		}

		// pluggable, draw power card, or powerprogression
		#region Draw Card

		static readonly IPowerCardDrawer DefaultCardDrawer = new DrawFromDeck();
		public IPowerCardDrawer CardDrawer { get; set; } = DefaultCardDrawer; // !!! public so tests can set it - find another way to set so we can make the set private

		public virtual Task<PowerCard> Draw( GameState gameState, Func<List<PowerCard>, Task> handleNotUsed ) 
			=> CardDrawer.Draw(this,gameState,handleNotUsed);

		public virtual Task<PowerCard> DrawMinor( GameState gameState ) 
			=> CardDrawer.DrawMinor( this, gameState, null );

		public virtual Task<PowerCard> DrawMajor( GameState gameState, int numberToDraw=4, bool forgetCard=true ) 
			=> CardDrawer.DrawMajor( this, gameState, null, forgetCard, numberToDraw ); // Instead of passing in null, could return Tupple with discard cards in it()

		#endregion

		// Used by Flame's Fury to detect new actions
		// Used by Observe The Ever-Changing World to distinguish between actions
		public Guid CurrentActionId; // !!! this might not work when we go to multi-player

		#region Purchase Cards

		// Plays cards from hand for cost
		public async Task PlayCardsFromHand( int? numberToPlay = null ) {
			int remainingToPlay = numberToPlay ?? NumberOfCardsPlayablePerTurn;

			PowerCard[] getPowerCardOptions() => Hand
				.Where( c => c.Cost <= Energy )
				.ToArray();

			PowerCard[] powerCardOptions;
			while(0 < remainingToPlay
				&& 0 < (powerCardOptions = getPowerCardOptions()).Length
			) {
				string prompt = $"Play power card (${Energy} / {remainingToPlay}):";
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
				elements = spirit.Elements.ToArray();
				// preparedElements = spirit.PreparedElements.ToArray();
				presence = spirit.Presence.SaveToMemento();
				hand      = spirit.Hand.ToArray();
				purchased = spirit.InPlay.ToArray();
				discarded = spirit.DiscardPile.ToArray();
				available = spirit.availableActions.ToArray();
				usedActions = spirit.usedActions.ToArray();
				usedInnates = spirit.usedInnates.ToArray();
			}
			public void Restore(Spirit spirit) {
				spirit.Energy = energy;
				InitFromArray( spirit.Elements, elements);
				// InitFromArray( spirit.PreparedElements, preparedElements); // !!! 
				spirit.Presence.LoadFrom(presence);
				spirit.Hand.SetItems( hand );
				spirit.InPlay.SetItems( purchased );
				spirit.DiscardPile.SetItems( discarded );
				spirit.availableActions.SetItems( available );
				spirit.usedActions.SetItems( usedActions );
				spirit.usedInnates.SetItems( usedInnates );
			}
			static void InitFromArray(ElementCounts dict, KeyValuePair<Element,int>[] array ) {
				dict.Clear(); 
				foreach(var p in array) dict[p.Key]=p.Value;
			}
			readonly int energy;
			readonly KeyValuePair<Element,int>[] elements;
			// readonly KeyValuePair<Element,int>[] preparedElements;
			readonly IMemento<SpiritPresence> presence;
			readonly PowerCard[] hand;
			readonly PowerCard[] purchased;
			readonly PowerCard[] discarded;
			readonly IActionFactory[] available;
			readonly IActionFactory[] usedActions;
			readonly InnatePower[] usedInnates;
		}
		#endregion

		#region Power Plug-ins

		public IDamageApplier CustomDamageStrategy = null; // Fires Fury Plugs in a custom +1 bonus damage

		public virtual InvaderGroup BuildInvaderGroupForPowers( GameState gs, Space space ) {
			return new InvaderGroup( 
				gs.Tokens[ space ],
				new DestroyInvaderStrategy( gs, gs.Fear.AddDirect, Cause.Power ),
				CustomDamageStrategy
			);
		}

		// overriden by Bringer, Bringer's BuildInvaderGroupForPower uses this.
		public virtual Task DestroyInvaderForPowers( GameState gs, Space space, int count, Token token ) {
			return gs.Tokens.DestroyIslandToken(space,count,token, Cause.Power);
		}

		/// <summary>Hook for Grinning Trickster to add additional strife for power</summary>
		public virtual Task AddStrife( TargetSpaceCtx ctx, Token invader ) {
			ctx.Tokens.AddStrifeTo( invader );
			return Task.CompletedTask;
		}

		// Overriden by Trickster because it costs them presence
		public virtual async Task RemoveBlight( TargetSpaceCtx ctx ) {
			if(ctx.Blight.Any)
				await ctx.GameState.AddBlight( ctx.Space, -1 );
		}

		public virtual TokenPusher PushFactory( TargetSpaceCtx ctx ) => new TokenPusher( ctx );
		public virtual TokenGatherer GatherFactory( TargetSpaceCtx ctx ) => new TokenGatherer( ctx );

		#endregion

		#region Tarteting / Range

		public ICalcSource SourceCalc = new DefaultSourceCalc();
		public ICalcRange RangeCalc = new DefaultRangeCalculator();

		// Only Called from TargetSpaceAttribute
		// !!! Also, some things may be calling GetTargetOptions directly and skipping over this bit - preventing Shadow from paying their energy
		public virtual Task<Space> TargetsSpace( 
			GameState gameState, 
			string prompt, 
			From from, 
			Terrain? sourceTerrain, 
			int range, 
			string filterEnum,
			TargettingFrom powerType
		) {
			if(prompt == null) prompt = "Target Space.";
			IEnumerable<Space> spaces = GetTargetOptions( gameState, range, filterEnum, powerType, from, sourceTerrain );
			return this.Action.Decision( new Decision.TargetSpace( prompt, spaces, Present.Always ));
		}

		public IEnumerable<Space> GetTargetOptions( 
			GameState gameState, 
			int range, 
			string filterEnum, 
			TargettingFrom powerType,

			From sourceEnum, 
			Terrain? sourceTerrain
		) {
			IEnumerable<Space> source = SourceCalc.FindSources( this.Presence, sourceEnum, sourceTerrain );
			return RangeCalc.GetTargetOptionsFromKnownSource( this, gameState, range, filterEnum, powerType, source );
		}

		#endregion

	}

}