namespace SpiritIsland.JaggedEarth;

[InnatePower(Name), Fast, FromPresence(1)]
public class ObserveTheEverChangingWorld {

	public const string Name = "Observe the Ever-Changing World";

	[InnateTier("1 moon","Prepare 1 Element Marker")]
	static public Task Option1(TargetSpaceCtx ctx ) {
		var smoa = (ShiftingMemoryOfAges)ctx.Self;
		return smoa.PrepareElement(ObserveTheEverChangingWorld.Name);
	}

	[InnateTier("2 moon,1 air","Instead, after each of the next three Actions that change which pieces are in atarget land, Prepare 1 Element Marker.")]
	static public Task Option2(TargetSpaceCtx ctx ) {
		ctx.Space.Init( new ObserveWorldMod( ctx ), 3 );
		return Task.CompletedTask;
	}

}

/// <summary>
/// Reminder Token that sits on a space and generates a Prepared Element for each action (up to 3) that the tokens change in that space.
/// </summary>
public class ObserveWorldMod( TargetSpaceCtx ctx ) 
	: ISpaceEntity
	, IToken
	, IHandleTokenAdded
	, IHandleTokenRemoved
	, IEndWhenTimePasses // :sadface: I want this to live between rounds.
{
	ITokenClass IToken.Class => Token.Element;

	public bool HasTag(ITag tag) => Token.Element.HasTag(tag);
	public string Text => ObserveTheEverChangingWorld.Name;

	public Img Img => Token.Element.Img;

	public void HandleTokenAdded( Space to, ITokenAddedArgs args ) => Check( to );

	public void HandleTokenRemoved( Space from, ITokenRemovedArgs args ) => Check( from );

	void Check( Space space ) {
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
			await _spirit.PrepareElement( space.SpaceSpec.Label );
		} );

	}

	#region private

	string _tokenSummary = ctx.Space.Summary;

	readonly ShiftingMemoryOfAges _spirit = (ShiftingMemoryOfAges)ctx.Self;
	readonly HashSet<ActionScope> _appliedToTheseActions = [];

	#endregion


}