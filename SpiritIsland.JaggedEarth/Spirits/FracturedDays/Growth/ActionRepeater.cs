using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.JaggedEarth {

	/// <summary>
	/// Binds multiple actions together and lets them be repeated as a group.
	/// </summary>
	public class ActionRepeater {

		public readonly int repeats;
		readonly List<GrowthActionFactory> factories = new List<GrowthActionFactory>();

		public int currentRepeats;

		public ActionRepeater(int repeats) { this.repeats = repeats; }

		public void Register( GrowthActionFactory factory) {
			factories.Add(factory);
		}

		public void BeginAction() {
			if(currentRepeats == 0) 
				currentRepeats = repeats;
		}

		public void EndAction( Spirit spirit ) {
			--currentRepeats;

			if(currentRepeats > 0)
				Restore( spirit );
			else
				CleanUp( spirit );
		}

		void Restore(Spirit spirit ) {
			var remaining = spirit.GetAvailableActions(Phase.Growth).ToArray();
			foreach(var factory in factories)
				if( !remaining.Contains(factory) )
					spirit.AddActionFactory( factory );
		}

		void CleanUp(Spirit spirit ) {
			var remaining = spirit.GetAvailableActions(Phase.Growth).ToArray();
			foreach(var factory in factories)
				if( remaining.Contains(factory) )
					spirit.RemoveFromUnresolvedActions( factory );
		}

		public GrowthActionFactory Bind( GrowthActionFactory inner ) => new RepeatableActionFactory( inner, this );

	}

}
