using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : MovePresence {

		public RangeCriteria rc;

		#region constructors

		public PlacePresence(
			Spirit spirit,
			int range
		):base(spirit){
			static bool IsNotOcean(Space s) => s.Terrain != Terrain.Ocean;
			rc = new RangeCriteria(range,IsNotOcean);
			this.referenceSpaces = referenceSpaces ?? spirit.Presence;
		}

		public PlacePresence(
			Spirit spirit,
			int range,
			Func<Space, bool> isValid
		) : base(spirit)
		{
			rc = new RangeCriteria(range, isValid ?? throw new ArgumentNullException(nameof(isValid)));
			this.referenceSpaces = referenceSpaces ?? spirit.Presence;
		}

		#endregion

		public override Space[] Options => options ??= CalculateOptions();
		Space[] options;
		Space[]  CalculateOptions() => PresenceCalculator.PresenseToPlaceOptions(
			referenceSpaces, 
			this.rc
		);

		readonly IEnumerable<Space> referenceSpaces;

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

		public void Apply(List<GrowthAction> growthActions) {
			var placePresence = growthActions
				.OfType<MovePresence>()
				.ToArray()[focus];
			Update(placePresence);
			placePresence.Apply();
		}

		protected virtual void Update(MovePresence pp) {
			pp.PlaceOnSpace = placeOnSpace;
			pp.Source = source;
		}
	}

}
