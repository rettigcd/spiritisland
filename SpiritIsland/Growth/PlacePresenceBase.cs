using System.Linq;

namespace SpiritIsland {

	public abstract class PlacePresenceBase : GrowthAction {

		public Track Source { get; set; }

		public Space Target { 
			get{ return target; }
			set{ 
				target = value; 
				ValidateTarget();
			}
		}
		Space target;

		public override bool IsResolved => Target != null && Source != default;

		public override void Apply() {
			TakeFromSource();
			PlaceOnTarget();
			spirit.MarkResolved(this);
		}

		void TakeFromSource() {
			switch (Source) {
				case Track.Card:
				spirit.RevealedCardSpaces++;
				break;
				case Track.Energy:
				spirit.RevealedEnergySpaces++;
				break;
			}
		}

		void PlaceOnTarget() {
			this.spirit.Presence.Add(Target);
		}

		void ValidateTarget() {
			var options = Options; // if Options is dynamic, cache...
			bool isValidTarget = options.Contains(Target);

			if (!isValidTarget)
				throw new InvalidPresenceLocation(Target.Label, options.Select(bs => bs.Text).ToArray());
		}

	}

}
