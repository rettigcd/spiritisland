using System;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class DestroyInvaderStrategy {

		public DestroyInvaderStrategy( Action<FearArgs> addFear, Cause destructionSource ) {
			if(destructionSource == Cause.None) 
				throw new ArgumentException("if we are destroying things, there must be a cause");
			this.addFear = addFear;
			this.destructionSource = destructionSource;
		}

		public virtual Task OnInvaderDestroyed( Space space, Token specific ) {
			if(specific == Invader.City[0])
				AddFear( space, 2 );
			if(specific == Invader.Town[0])
				AddFear( space, 1 );
			return Task.CompletedTask;
		}

		protected void AddFear( Space space, int count ) => addFear( new FearArgs { count = count, cause = this.destructionSource, space = space } );

		readonly Action<FearArgs> addFear;
		readonly Cause destructionSource;

	}

}
