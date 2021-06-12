using System.Linq;

namespace SpiritIsland {

	public abstract class PlacePresenceBase : GrowthAction {

		public abstract Space[] Options { get; }

		public string PlaceOnSpace { get; set; }

		public Track Source { get; set; }

		public override bool IsResolved => PlaceOnSpace != null && Source != default;

		public override void Apply() {

			static string FormatOption(Space bs) => bs.Label;

			var opts = Options;
			var option = Options
				.FirstOrDefault(o => FormatOption(o) == PlaceOnSpace);

			if( option == null )
				throw new InvalidPresenceLocation(PlaceOnSpace,Options.Select(FormatOption).ToArray());

			this.spirit.Presence.Add(option);

			switch(Source) {
				case Track.Card:
					spirit.RevealedCardSpaces++;
					break;
				case Track.Energy:
					spirit.RevealedEnergySpaces++;
					break;
			}

			PlaceOnSpace = null;

			spirit.MarkResolved( this );
		}

	}

}
