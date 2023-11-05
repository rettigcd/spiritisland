namespace SpiritIsland;

/// <summary>
/// Commands the Beasts (non-events) for 1 space.
/// </summary>
internal class CommandBeasts : IActOn<TargetSpaceCtx> {

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
		if( count == 0 ) return;

		// Damage
		count -= await PartialDamageToInvaders( ctx, count);

		// Push
		count -= (await ctx.PushUpTo(count, Token.Beast)).Length;

		// Fear
		if( startedWithInvaders )
			ctx.AddFear( count );

	}

	#region private

	CountDictionary<Space> _originalBeastCounts;

	static async Task<int> PartialDamageToInvaders( TargetSpaceCtx ctx, int damage ) {
		if(damage == 0) return 0; // not necessary, just saves some cycles

		int badlandCount = ctx.Badlands.Count;

		var totalDamage = ctx.BonusDamageForAction( damage );
		int damageDone = await ctx.Invaders.UserSelectedPartialDamage( damage, ctx.Self );
		totalDamage.TrackDamageDone( damageDone );

		return damageDone == 0 
			? 0 // no damage done
			: Math.Max( 1, damageDone - badlandCount ); // some damage done, remove badland damage.

	}

	#endregion
}

class CommandBeastAction : IActionFactory {
	public string Name => "Command Beasts";

	public string Text => Name;

	public async Task ActivateAsync( SelfCtx ctx ) {
		Used = true;

		await using var actionScope = await ActionScope.Start(ActionCategory.Special); // replace generic scope passed in.
		GameCtx gameCtx = new GameCtx( GameState.Current );
		await new CommandBeasts().In().EachActiveLand().ActAsync( gameCtx );
	}
	public bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Fast;
	public bool Used { get; private set; }
}

public class TriggerCommandBeasts {

	readonly CommandBeastAction cmdAction = new CommandBeastAction();

	public Task QueueBeastCommand( GameState gameState ) {
		gameState.TimePasses_WholeGame += TimePasses_WholeGame;
		return Task.CompletedTask;
	}

	Task TimePasses_WholeGame( GameState gameState ) {
		if(!cmdAction.Used)
			gameState.Spirits[0].AddActionFactory( cmdAction );
		return Task.CompletedTask;
	}
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