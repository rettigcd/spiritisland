﻿using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class BlightCardBase : IBlightCard {

		protected BlightCardBase(string name, int side2BlightPerPlayer ) {
			Name = name;
			this.startingBlightPerPlayer = 2;
			this.side2BlightPerPlayer = side2BlightPerPlayer;
		}

		public string Name { get; }
		public bool IslandIsBlighted { get; set; }

		public void OnGameStart( GameState gs ) {
			gs.blightOnCard = startingBlightPerPlayer * gs.Spirits.Length + 1; // +1 from Jan 2021 errata
		}

		public void OnBlightDepleated( GameState gs ) {
			if(IslandIsBlighted)
				GameOverException.Lost("Blighted Island-"+Name);

			IslandIsBlighted = true;
			gs.blightOnCard += side2BlightPerPlayer * gs.Spirits.Length;
		}

		protected virtual void Side2Depleted(GameState gameState) => GameOverException.Lost( "Blighted Island-" + Name );

		public async Task OnStartOfInvaders( GameState gs ) {
			if( IslandIsBlighted )
				await BlightAction( gs );
		}

		protected abstract Task BlightAction( GameState gs );

		#region private

		readonly int startingBlightPerPlayer;
		readonly int side2BlightPerPlayer;

		#endregion
	}


}
