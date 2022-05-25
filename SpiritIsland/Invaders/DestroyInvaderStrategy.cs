namespace SpiritIsland;

public class DestroyInvaderStrategy {

	public DestroyInvaderStrategy( GameState gs, Action<FearArgs> addFear ) {
		this.gs = gs;
		this.addFear = addFear;
	}

	public virtual async Task OnInvaderDestroyed( Space space, HealthToken token, bool fromRavage, Guid actionId ) {

		var reason = fromRavage ? RemoveReason.DestroyedInBattle : RemoveReason.Destroyed;

		await gs.Tokens[space].Destroy( token, 1, actionId );

		// !!! see if we can invoke this through the Token-Publish API instead - so we can make TokenRemovedArgs internal to Island_Tokens class
		await gs.Tokens.TokenRemoved.InvokeAsync( new TokenRemovedArgs( gs, token, reason, actionId ) {
			Space = space,
			Count = 1,
		} );

		// Don't assert token is destroyed (from damage) because it is possible to destory healthy tokens

		AddFear( space, token.Class.FearGeneratedWhenDestroyed, true );
	}

	protected void AddFear( Space space, int count, bool fromInvaderDestruction ) => addFear( new FearArgs { 
		count = count, 
		FromDestroyedInvaders = fromInvaderDestruction, // this is the destruction that Dread Apparitions ignores.
		space = space 
	} );

	readonly GameState gs;
	readonly Action<FearArgs> addFear;

}