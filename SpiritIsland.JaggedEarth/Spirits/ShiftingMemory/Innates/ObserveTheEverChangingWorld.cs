namespace SpiritIsland.JaggedEarth;

[InnatePower(Name), Fast, FromPresence(1)]
public class ObserveTheEverChangingWorld {

	public const string Name = "Observe the Ever-Changing World";

	[InnateOption("1 moon","Prepare 1 Element Marker")]
	static public Task Option1(TargetSpaceCtx ctx ) {
		var smoa = (ShiftingMemoryOfAges)ctx.Self;
		return smoa.PrepareElement(ObserveTheEverChangingWorld.Name);
	}

	[InnateOption("2 moon,1 air","Instead, after each of the next three Actions that change which pieces are in atarget land, Prepare 1 Element Marker.")]
	static public Task Option2(TargetSpaceCtx ctx ) {
		ctx.Tokens.Init( new ObserveWorldMod( ctx ), 3 );
		return Task.CompletedTask;
	}

}


public class ObserveWorldMod : ISpaceEntity
	, IToken
	, IHandleTokenAdded
	, IHandleTokenRemoved
{
	string _tokenSummary;

	public IEntityClass Class => Token.Element;
	public string Text => ObserveTheEverChangingWorld.Name;

	public Img Img => Token.Element.Img;

	readonly ShiftingMemoryOfAges _spirit;
	readonly HashSet<ActionScope> _appliedToTheseActions = new HashSet<ActionScope>();

	public ObserveWorldMod( TargetSpaceCtx ctx ) {
		_spirit = (ShiftingMemoryOfAges)ctx.Self;
		_tokenSummary = ctx.Tokens.Summary;
	}

	public Task HandleTokenAdded( ITokenAddedArgs args ) {
		Check( args.AddedTo );
		return Task.CompletedTask;
	}
	public Task HandleTokenRemoved( ITokenRemovedArgs args ) {
		Check( args.RemovedFrom );
		return Task.CompletedTask;
	}

	void Check( SpaceState space ) {
		var actionScope = ActionScope.Current;
		if(    _appliedToTheseActions.Contains( actionScope ) // already did this action 
			|| _tokenSummary == space.Summary   // no change in tokens
		)
			return;

		if(actionScope == default)
			throw new InvalidOperationException( "Can't use default action-scope" );

		_appliedToTheseActions.Add( actionScope ); // limit to 1 change per action
		_tokenSummary = space.Summary;

		// This web page states SMOA shouldn't get the element until after the Action completes.
		// https://boardgamegeek.com/thread/2399380/shifting-memory-observe-ever-changing-world
		actionScope.AtEndOfThisAction(async _ => {
			space.Adjust( this, -1 );
			await _spirit.PrepareElement( space.Space.Label );
		} );


	}

}