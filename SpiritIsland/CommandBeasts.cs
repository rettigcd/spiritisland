namespace SpiritIsland;

/// <summary>
/// Starts as an event Handler on the Invader cards
/// Moves to an action that runs when TimePasses
/// Added to 1st Spirits ActionFactory list until they use it.
/// </summary>
public class CommandBeasts : IActionFactory, IRunWhenTimePasses, IHaveMemento {

	public const string Stage1 = "Command Beasts (I)";
	public const string Stage2 = "Command Beasts (II)";

	#region Static Setup

	static public void Setup( GameState gameState ) {
		// If there are no Event cards, compensate with Command-the-Beasts

		gameState.InvaderDeck
			.UnrevealedCards
			.First( x => x.InvaderStage == 2 )
			.CardRevealed += new CommandBeasts(Stage1).OnCardRevealed;

		// Find 1st card of last Level-3 group
		var cards = gameState.InvaderDeck.UnrevealedCards;
		int i = cards.Count;
		while(cards[i - 1].InvaderStage != 3) --i; // While prev is not level 3, backup - ends on card following level 3 group
		while(cards[i - 1].InvaderStage == 3) --i; // While prev is level 3, backup - ends on 1st level 3 card in last group
		cards[i].CardRevealed += new CommandBeasts( Stage2 ).OnCardRevealed;
	}

	#endregion Static Setup

	public CommandBeasts( string name ) { Name = name; }

	public string Name { get; }

	public string Text => Name;

	public async Task ActivateAsync( Spirit _ ) {
		_used = true;
		await using ActionScope actionScope = await ActionScope.Start(ActionCategory.Special);
		await new CommandBeastsOn1Space().In().EachActiveLand().ActAsync( GameState.Current );
	}

	public bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Fast;

	#region IRunWhenTimePasses

	Task IRunWhenTimePasses.TimePasses( GameState gameState ) {
		if(!_used)
			gameState.Spirits[0].AddActionFactory( this );
		return Task.CompletedTask;
	}

	bool IRunWhenTimePasses.RemoveAfterRun => _used;

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Late;

	#endregion

	#region IHaveMemento

	object IHaveMemento.Memento {
		get => _used;
		set => _used = (bool)value;
	}

	#endregion IHaveMemento

	#region private

	/// <summary>
	/// Creates a new Command-the-Beasts Action and adds it to the 1st spirits actions until it is used.
	/// </summary>
	async Task OnCardRevealed( GameState gameState ) {
		await AllSpirits.Acknowledge( "Invader Deck Card Revealed", Name, this );
		gameState.AddTimePassesAction( this );
	}
	bool _used;

	#endregion private
}

/// <summary>
/// Commands-the-Beasts (non-events) for 1 space.
/// </summary>
internal class CommandBeastsOn1Space : IActOn<TargetSpaceCtx> {

	public string Description => "For each (original) beast on land, push, do 1 damage, or 1 fear if invaders are present.";

	public bool IsApplicable( TargetSpaceCtx ctx ) => _originalBeastCounts[ctx.Space] > 0;// use original, not current. Incase anything moved.

	public async Task ActAsync( TargetSpaceCtx ctx ) {

		// The first space/time it is called on, init original Beast positions
		_originalBeastCounts ??= GameState.Current.Spaces
				.ToDictionary( s => s.Space, s => s.Beasts.Count )
				.ToCountDict();

		// Using this order: Damage / Push / Fear(based on starting invaders)
		//	Pro:	Assists user. Can use as much damage as desired (this is probably top priority) and not worry about using fear prior to killing explorers.
		//	Pro:	Doesn't require user input on fear count.  Can just apply whatever remains as fear.
		//	Con:	Action appear done out of order. (but this is ok since it is really simultaneous

		bool startedWithInvaders = ctx.HasInvaders;

		int count = _originalBeastCounts[ctx.Space];
		if(count == 0) return;

		// Damage
		count -= await PartialDamageToInvaders( ctx, count );

		// Push
		await ctx.SourceSelector
			.AddGroup( count, Token.Beast )
			.Track( _ => --count )
			.PushUpToN( ctx.Self );

		// Fear
		if(startedWithInvaders)
			ctx.AddFear( count );

	}

	#region private

	CountDictionary<Space> _originalBeastCounts;

	static async Task<int> PartialDamageToInvaders( TargetSpaceCtx ctx, int damage ) {
		if(damage == 0) return 0; // not necessary, just saves some cycles

		int badlandCount = ctx.Badlands.Count;

		CombinedDamage combinedDamage = ctx.Tokens.CombinedDamageFor_Invaders( damage );

		int damageDone = await ctx.SourceSelector.AddAll( Human.Invader )
			.DoDamageAsync( ctx.Self, combinedDamage.Available, Present.Done );

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