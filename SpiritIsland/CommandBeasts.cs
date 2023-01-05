namespace SpiritIsland;

/// <summary>
/// Commands the Beasts (non-events) for 1 space.
/// </summary>
internal class CommandBeasts : IExecuteOn<TargetSpaceCtx> {

	public string Description => "For each (original) beast on land, push, do 1 damage, or 1 fear if invaders are present.";

	public bool IsApplicable( TargetSpaceCtx ctx ) => originalBeastCounts[ctx.Space] > 0;// use original, not current. Incase anything moved.

	public async Task Execute( TargetSpaceCtx ctx ) {

		// The first space/time it is called on, init original Beast positions
		if( originalBeastCounts == null )
			originalBeastCounts = ctx.GameState.AllActiveSpaces
				.ToDictionary( s => s.Space, s => s.Beasts.Count )
				.ToCountDict();

		// Using this order: Damage / Push / Fear(based on starting invaders)
		//	Pro:	Assists user. Can use as much damage as desired (this is probably top priority) and not worry about using fear prior to killing explorers.
		//	Pro:	Doesn't require user input on fear count.  Can just apply whatever remains as fear.
		//	Con:	Action appear done out of order. (but this is ok since it is really simultaneous

		bool startedWithInvaders = ctx.HasInvaders;

		int count = originalBeastCounts[ctx.Space];
		if( count == 0 ) return;

		// Damage
		count -= await PartialDamageToInvaders( ctx, count);

		// Push
		count -= (await ctx.PushUpTo(count, TokenType.Beast)).Length;

		// Fear
		if( startedWithInvaders )
			ctx.AddFear( count );

	}

	#region private

	CountDictionary<Space> originalBeastCounts;

	static async Task<int> PartialDamageToInvaders( TargetSpaceCtx ctx, int damage ) {
		if(damage == 0) return 0; // not necessary, just saves some cycles

		// !!! This is not correct, if card has multiple Damages, adds badland multiple times.
		int badlandCount = ctx.Badlands.Count;
		damage += badlandCount;

		int damageDone = await ctx.Invaders.UserSelectedPartialDamage( damage, ctx.Self, Invader.Explorer, Invader.Town, Invader.City );
		return damageDone == 0 
			? 0 // no damage done
			: Math.Max( 1, damageDone - badlandCount ); // some damage done, remove badland damage.

	}

	#endregion
}

class CommandBeastAction : IActionFactory {
	public string Name => "Command Beasts";

	public string Text => Name;

	public Task ActivateAsync( SelfCtx ctx ) {
		Used = true;
		var gameCtx = new GameCtx( ctx.GameState, ActionCategory.Special );
		return Cmd.InEachLand( new CommandBeasts() ).Execute( gameCtx );
	}
	public bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Fast;
	public bool Used { get; private set; }
}

public class TriggerCommandBeasts : IInvaderCard {

	readonly CommandBeastAction cmdAction = new CommandBeastAction();
	readonly GameState _gameState;

	public TriggerCommandBeasts( IInvaderCard inner, GameState gameState ) {
		this._inner = inner;
		_gameState = gameState;
	}

	void QueueBeastCommand() {
		_gameState.TimePasses_WholeGame += TimePasses_WholeGame;
	}

	Task TimePasses_WholeGame( GameState gameState ) {
		if(!cmdAction.Used)
			gameState.Spirits[0].AddActionFactory( cmdAction );
		return Task.CompletedTask;
	}

	#region IInvaderCard Properties

	readonly IInvaderCard _inner;

	public string Text => _inner.Text;
	public int InvaderStage => _inner.InvaderStage;

	public bool Skip { get => false; set => throw new NotImplementedException(); }
	public bool HoldBack { get => false; set => throw new NotImplementedException(); }

	public bool Flipped {
		get => _inner.Flipped;
		set => _inner.Flipped = value;
	}

	public void Flip() {
		QueueBeastCommand();
		_inner.Flip();
	}

	//	public bool MatchesCard( Space space ) => inner.MatchesCard( space );
	public bool MatchesCard( SpaceState space ) => _inner.MatchesCard( space );
	public Task Ravage( GameState gameState ) => _inner.Ravage( gameState );
	public bool HasEscalation {
		get => _inner.HasEscalation;
		set => _inner.HasEscalation = value;
	}

	#endregion
}

// class:
//   has method that gets exected at the each time-passes that puts the object in the Head Spirits Fast action unless it has been used.
//   has Fast Action that Triggers.Command

// Create Command Beast Card
//	Invader Action
//		Start CommandBeast Fast Action
//		Draw the Next explorer card and execute it.
//			OR
//		When configuring, configure 2 draws automatically there.`

/*
Add feature to Invader card to trigger it.
Replace Invader Card with augmented card.
Draw 2 cards.
*/