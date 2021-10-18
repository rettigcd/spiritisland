using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpiritIsland.JaggedEarth {

	class GrinningTricksterStirsUpTrouble : Spirit {

		public const string Name = "Grinning Trickster Stirs Up Trouble";
		public override string Text => Name;
		public override string SpecialRules => "A Real Flair for Discord - After one of your Powers adds strife in a land, you may pay 1 Energy to add 1 strife within Range-1 of that land."
			+ " Cleaning up Messes is a Drag - AFter one of your Powers Removes blight, Destory 1 of your presence.  Ignore this rule for Let's See What Happens";

		public GrinningTricksterStirsUpTrouble()
			:base(
				new SpiritPresence(
					new PresenceTrack(Track.Energy1,Track.MoonEnergy,Track.Energy2,Track.AnyEnergy,Track.FireEnergy,Track.Energy3),
					new PresenceTrack(Track.Card2,Track.PushDahan,Track.Card3,Track.Card3,Track.Card4,Track.AirEnergy,Track.Card5) // !!!
				)
				,PowerCard.For<ImpersonateAuthority>()
				,PowerCard.For<InciteTheMob>()
				,PowerCard.For<OverenthusiasticArson>()
				,PowerCard.For<UnexpectedTigers>()
			)
		{
			// Growth
			// Innates
		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Place presence on highest numbered land with dahan
			Presence.PlaceOn(board.Spaces.Where(s=>gameState.Tokens[s][TokenType.Dahan.Default]>0).Last());
			// and in land #4
			Presence.PlaceOn(board[4]);

		}

	}
}
