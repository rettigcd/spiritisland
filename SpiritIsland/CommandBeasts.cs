using SpiritIsland.Log;

namespace SpiritIsland;

/// <summary>
/// Starts as an event Handler on the Invader cards
/// Moves to an action that runs when TimePasses
/// Added to 1st Spirits ActionFactory list until they use it.
/// </summary>
public class CommandBeasts( string title ) 
	: IActionFactory
	, IRunWhenTimePasses
	, IHaveMemento
	, ILogEntry
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
		int i =0; while(cards[i].InvaderStage != 2) ++i;
		if(0<i && cards[i-1].InvaderStage==3) --i; // Brandenburg Prussia
		cards[i].CardRevealed += new CommandBeasts(Stage2).OnCardRevealed;

		// Stage 3 - Find 1st card of last Level-3 group
		i = cards.Count;
		while(cards[i - 1].InvaderStage != 3) --i; // While prev is not level 3, backup - ends on card following level 3 group
		while(cards[i - 1].InvaderStage == 3) --i; // While prev is level 3, backup - ends on 1st level 3 card in last group
		cards[i].CardRevealed += new CommandBeasts( Stage3 ).OnCardRevealed;
	}

	#endregion Static Setup

	string IOption.Text => Title;
	public string Title { get; } = title;

	public async Task ActivateAsync( Spirit _ ) {
		_used = true;
		GameState gs = GameState.Current;
		await using ActionScope actionScope = await ActionScope.Start(ActionCategory.Special);
		await new CommandBeastsOn1Space().In().EachActiveLand().ActAsync( gs );
		gs.ReminderCards.Remove(this);
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
	Task OnCardRevealed( GameState gameState ) {
		// Memento only called when in TimePasses collection
		_used = false; // this is needed but I can't figure out why
		gameState.Log( this );

		gameState.ReminderCards.Add(this);
		gameState.AddTimePassesAction( this );
		return Task.CompletedTask;
	}
	bool _used;

	#endregion private
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

		int damageDone = await ctx.SourceSelector.UseQuota(new Quota().AddAll( Human.Invader ))
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