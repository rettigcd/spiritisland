using SpiritIsland.PowerCards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : MovePresence {

		public int Range { get; }
		public Func<Space,GameState, bool> IsValid { get; }

		#region constructors

		public PlacePresence( int range ){
			static bool IsNotOcean(Space s,GameState _) => s.Terrain != Terrain.Ocean;
			Range = range;
			IsValid = IsNotOcean;
		}

		public PlacePresence(
			int range,
			Func<Space, GameState, bool> isValid
		){
			Range = range;
			IsValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
		}

		#endregion

		public override Space[] Options => CalculateOptions();
		Space[]  CalculateOptions() {
			bool SpaceIsValid(Space space) => IsValid(space,gameState);
			return referenceSpaces
				.SelectMany(s => s.SpacesWithin(this.Range))
				.Distinct()
				.Where(SpaceIsValid)
				.ToArray();
		}

		IEnumerable<Space> referenceSpaces => spirit.Presence;

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
