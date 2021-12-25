using System.Threading.Tasks;

namespace SpiritIsland {

	public abstract class BlightCardBase : IBlightCard {

		protected BlightCardBase(string name, int side2BlightPerPlayer ) {
			this.name = name;
			this.startingBlightPerPlayer = 2;
			this.side2BlightPerPlayer = side2BlightPerPlayer;
		}

		public bool IslandIsBlighted { get; set; }

		public void OnGameStart( GameState gs ) {
			gs.blightOnCard = startingBlightPerPlayer * gs.Spirits.Length + 1; // +1 from Jan 2021 errata
		}

		public void OnBlightDepleated( GameState gs ) {
			if(IslandIsBlighted)
				GameOverException.Lost("Blighted Island-"+name);

			IslandIsBlighted = true;
			gs.blightOnCard += side2BlightPerPlayer * gs.Spirits.Length;
		}

		public async Task OnStartOfInvaders( GameState gs ) {
			if( IslandIsBlighted )
				await BlightAction( gs );
		}

		protected abstract Task BlightAction( GameState gs );

		#region private

		readonly string name;
		readonly int startingBlightPerPlayer;
		readonly int side2BlightPerPlayer;

		#endregion
	}


}
