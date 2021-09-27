using System;

namespace SpiritIsland.Tests {

	// Test spirit that has lots of energy and you can select the card they start with
	class CardSpirit : Spirit {

		public CardSpirit(PowerCard powerCard):base(
			new MyPresence(new PresenceTrack(Track.Energy5,Track.Energy9),new PresenceTrack(Track.Card1,Track.Card2))
			,powerCard
		) {
			GrowthOptions = new GrowthOption[] { new GrowthOption( new ReclaimAll() ) };
		}

		public override string Text => "CardPlayTestSpirit";
		public override string SpecialRules => "none";

		protected override void InitializeInternal( Board board, GameState _ ) {
			// Has sacred site on space 5
			var space = board[5];
			Presence.PlaceOn(space);
			Presence.PlaceOn(space);
		}

		static public (VirtualCardUser, SpiritGameStateCtx) SetupGame( PowerCard powerCard, Action<GameState> modGameState = null ) {
			var spirit = new CardSpirit( powerCard );
			var gs = new GameState( spirit, Board.BuildBoardA() ) {
				InvaderDeck = new InvaderDeck( null ) // Same order every time
			};
			modGameState?.Invoke( gs );
			_ = new SinglePlayer.SinglePlayerGame( gs );

			var user = new VirtualCardUser( spirit );
			var starterCtx = new SpiritGameStateCtx( spirit, gs, Cause.None );
			return (user,starterCtx);
		}

	}

	public class VirtualCardUser : VirtualUser {

		public VirtualCardUser(Spirit spirit ) : base( spirit ) { }

		public void Grows() {
			SelectsGrowthOption( "ReclaimAll" );
			ReclaimsAll();
		}

	}

}
