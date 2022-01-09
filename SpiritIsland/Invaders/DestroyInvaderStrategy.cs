using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class DestroyInvaderStrategy {

		public DestroyInvaderStrategy( GameState gs, Action<FearArgs> addFear ) {
			this.gs = gs;
			this.addFear = addFear;
		}

		public virtual async Task OnInvaderDestroyed( Space space, Token token, bool fromRavage ) {

			var reason = fromRavage ? RemoveReason.DestroyedInBattle : RemoveReason.Destroyed;

			// !!! see if we can invoke this through the Token-Publish API instead - so we can make TokenRemovedArgs internal to Island_Tokens class
			await gs.Tokens.TokenRemoved.InvokeAsync( new TokenRemovedArgs( gs, token, reason ) {
				Space = space,
				Count = 1,
			} );

			if(token == Invader.City[0])
				AddFear( space, 2, true );
			if(token == Invader.Town[0])
				AddFear( space, 1, true );

		}

		protected void AddFear( Space space, int count, bool fromInvaderDestruction ) => addFear( new FearArgs { 
			count = count, 
			FromDestroyedInvaders = fromInvaderDestruction, // this is the destruction that Dread Apparitions ignores.
			space = space 
		} );

		readonly GameState gs;
		readonly Action<FearArgs> addFear;

	}

}
