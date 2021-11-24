using System;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class DestroyInvaderStrategy {

		public DestroyInvaderStrategy( GameState gs, Action<FearArgs> addFear, Cause destructionSource ) {
			if(destructionSource == Cause.None) 
				throw new ArgumentException("if we are destroying things, there must be a cause");
			this.gs = gs;
			this.addFear = addFear;
			this.destructionSource = destructionSource;
		}

		public virtual async Task OnInvaderDestroyed( Space space, Token token ) {

//			gs.Tokens.DestroyIslandToken( space, 1, token, destructionSource );

			await gs.Tokens.TokenDestroyed.InvokeAsync( gs, new TokenDestroyedArgs {
				Token = token.Generic,
				Space = space,
				count = 1,
				Cause = destructionSource
			} );

			if(token == Invader.City[0])
				AddFear( space, 2 );
			if(token == Invader.Town[0])
				AddFear( space, 1 );

		}

		protected void AddFear( Space space, int count ) => addFear( new FearArgs { count = count, cause = this.destructionSource, space = space } );

		readonly GameState gs;
		readonly Action<FearArgs> addFear;
		readonly Cause destructionSource;

	}

}
