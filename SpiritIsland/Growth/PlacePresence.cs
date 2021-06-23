using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : PlacePresenceBase {

		readonly int range;
		readonly Func<Space,GameState, bool> isValid;

		#region constructors

		public PlacePresence( int range ){
			static bool IsNotOcean(Space s,GameState _) => s.IsLand;
			this.range = range;
			isValid = IsNotOcean;
		}

		public PlacePresence(
			int range,
			Func<Space, GameState, bool> isValid
		){
			this.range = range;
			this.isValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
		}

		#endregion

		public override IOption[] Options => spirit.Presence
			.SelectMany(s => s.SpacesWithin(this.range))
			.Distinct()
			.Where(SpaceIsValid)
			.ToArray();

		bool SpaceIsValid(Space space) => isValid(space,gameState);

	}

	public class ResolvePlacePresence : IResolver {

		readonly string placeOnSpace;
		readonly Track source;
		readonly int focus;

		public ResolvePlacePresence(string placeOnSpace, int focus, Track source) {
			this.placeOnSpace = placeOnSpace;
			this.source = source;
			this.focus = focus;
		}

		public void Apply(List<IAction> growthActions) {
			var placePresence = growthActions
				.OfType<PlacePresenceBase>()
				.ToArray()[focus];
			Update(placePresence);
			placePresence.Apply();
		}

		protected virtual void Update(PlacePresenceBase pp) {
			pp.Target = (Space)pp.Options.Single(s=>s.Text==placeOnSpace);
			pp.Source = source;
		}
	}

}
