﻿
namespace SpiritIsland {

	public class Invaders {

		// !! This wrapper class (around TokenCountDictionary) acts more like an Extension Method Class

		#region constructor

		public Invaders( GameState gs ) {
			this.gs = gs;
			this.gs.TimePasses_WholeGame += Heal;
		}

		#endregion

		public InvaderGroup On( Space targetSpace, Cause cause ) {
			return new InvaderGroup( 
				gs.Tokens[targetSpace], 
				new DestroyInvaderStrategy( gs, gs.Fear.AddDirect, cause )
			);
		}

		#region private

		void Heal( GameState obj ) {
			foreach(var space in gs.Tokens.Keys)
				InvaderGroup.HealTokens( gs.Tokens[space] );
		}

		readonly GameState gs;

		#endregion

	}

}
