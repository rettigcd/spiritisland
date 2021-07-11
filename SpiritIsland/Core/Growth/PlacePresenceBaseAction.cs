using System.Linq;

namespace SpiritIsland.Core {

	public class PlacePresenceBaseAction : BaseAction {

		public PlacePresenceBaseAction(Spirit spirit,GameState gs,Space[] destinationOptions)
			:base(spirit,gs)
		{
			var ctx = new PlacePresenceCtx{ spirit=spirit, destinationOptions = destinationOptions };
			engine.decisions.Push(new SelectPlacePresenceLocation(ctx));
			engine.decisions.Push(new SelectPresenceToPlace(ctx));
			engine.actions.Add(ctx);
		}

		class SelectPresenceToPlace : IDecision {
			readonly PlacePresenceCtx ctx;
			public SelectPresenceToPlace(PlacePresenceCtx ctx){ this.ctx = ctx; }
			public string Prompt => "Select Presence to place.";

			public IOption[] Options => new IOption[]{ Track.Energy,Track.Card };

			public void Select( IOption option ) {
				ctx.source = (Track)option;
			}
		}

		class SelectPlacePresenceLocation : IDecision {
			readonly PlacePresenceCtx ctx;

			public SelectPlacePresenceLocation(PlacePresenceCtx ctx){  this.ctx = ctx; }

			public string Prompt => "Where would you like to place your presence?";

			public IOption[] Options => ctx.destinationOptions;

			public void Select( IOption option ) {
				ctx.target = (Space)option;
			}

		}

		class PlacePresenceCtx : IAtomicAction {
			public Track source;
			public Space target;

			public Space[] destinationOptions;
			public Spirit spirit;
			public void Apply( GameState gameState ) {
				TakeFromSource();
				PlaceOnTarget();
			}

			void TakeFromSource() {
				if(source == Track.Card)
					spirit.RevealedCardSpaces++;
				else if(source == Track.Energy)
					spirit.RevealedEnergySpaces++;
			}

			void PlaceOnTarget() {
				this.spirit.Presence.Add(target);
			}

		}


	}

}
