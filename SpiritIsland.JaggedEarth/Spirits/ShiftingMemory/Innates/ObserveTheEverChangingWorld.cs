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


public class ObserveWorldMod : Token, IHandleTokenAdded, IHandleTokenRemoved {

	// !!! It seems like adding / removing presence tokens should trigger this also, but I don't think it triggers the Token Added/Removed event.

	static int _total = 0;
	readonly int _index;
	string _tokenSummary;

	public TokenClass Class => TokenType.Element;
	public string Text => ObserveTheEverChangingWorld.Name;

	public string SpaceAbreviation => $"AnyElement({_index})";


	readonly ShiftingMemoryOfAges _spirit;
	readonly HashSet<UnitOfWork> _appliedToTheseActions = new HashSet<UnitOfWork>();

	public ObserveWorldMod( TargetSpaceCtx ctx ) {
		_spirit = (ShiftingMemoryOfAges)ctx.Self;
		_index = _total++;
		_tokenSummary = ctx.Tokens.Summary;
	}

	public Task HandleTokenAdded( ITokenAddedArgs args ) {
		Check( args.AddedTo, args.ActionScope );
		return Task.CompletedTask;
	}
	public Task HandleTokenRemoved( ITokenRemovedArgs args ) {
		Check( args.RemovedFrom, args.ActionScope );
		return Task.CompletedTask;
	}

	void Check( SpaceState space, UnitOfWork actionScope ) {
		if(    _appliedToTheseActions.Contains( actionScope ) // already did this action 
			|| _tokenSummary == space.Summary   // no change in tokens
		)
			return;

		if(actionScope == default)
			throw new InvalidOperationException( "Can't use default action-scope" );

		_appliedToTheseActions.Add( actionScope ); // limit to 1 change per action
		_tokenSummary = space.Summary;

		actionScope.AtEndOfThisAction(async _ => {
			space.Adjust( this, -1 );

			// !!! This web page states SMOA shouldn't get the element until after the Action completes.
			// https://boardgamegeek.com/thread/2399380/shifting-memory-observe-ever-changing-world
			await _spirit.PrepareElement( space.Space.Label );

		} );


	}

}