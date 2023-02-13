namespace SpiritIsland.Tests.Spirits.Thunder;

public class ThunderCards {

	public ThunderCards() { 

		spirit = new Thunderspeaker { Energy = 20 };
		User = new VirtualUser( spirit );

		// Given: empty board
		a = Board.BuildBoardA();
		gs = new GameState( spirit, a );

		// And: Spirit in spot 1
		spirit.Presence.PlaceOn(a[1], gs).Wait();

		action = spirit.Gateway;
	}

	protected void When_ActivateCard( string cardName ) {

		async Task Run() {
			try {
				var action = gs.StartAction( ActionCategory.Spirit_Power ); // !!! dispose
				await spirit.Hand.Single( x => x.Name == cardName ).ActivateAsync( spirit.BindMyPowers() ); 
			}
			catch(Exception ex) {
				_ = ex.ToString();
			}
		}


		_ = Run();
	}

	protected readonly Spirit spirit;
	protected readonly VirtualUser User;
	protected readonly Board a;
	protected readonly GameState gs;
	protected readonly UserGateway action;

}
