﻿using System.Linq;

namespace SpiritIsland.Core {

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
		}

		void TakeFromSource() {
			if(Source == Track.Card)
				spirit.RevealedCardSpaces++;
			else if(Source == Track.Energy)
				spirit.RevealedEnergySpaces++;
		}

		void PlaceOnTarget() {
			this.spirit.Presence.Add(Target);
		}

		void ValidateTarget() {
			var options = Options; // if Options is dynamic, cache...
			bool isValidTarget = options.Contains(Target);

			if (!isValidTarget)
				throw new InvalidPresenceLocationException(Target.Label, options.Select(bs => bs.Text).ToArray());
		}

		public Spirit Spirit => spirit; // public for ResolvedPlacePresence, remove when not needed anymore

	}

}
