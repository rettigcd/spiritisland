using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Tests.Growth {

	public class ResolvePlacePresence {

		readonly string placeOnSpace;
		readonly Track source;
		readonly int focus;

		public ResolvePlacePresence(string placeOnSpace, int focus, Track source) {
			this.placeOnSpace = placeOnSpace;
			this.source = source;
			this.focus = focus;
		}

		public void Apply(List<IAction> growthActions) {
			PlacePresenceBase action = growthActions
				.OfType<PlacePresenceBase>()
				.ToArray()[focus];
			Update(action);
			action.Apply();
			action.Resolved(action.Spirit);
		}

		protected virtual void Update(PlacePresenceBase pp) {
			pp.Source = source;
			pp.Target = (Space)pp.Options.Single(s=>s.Text==placeOnSpace);
		}
	}


	public class SpyOnPlacePresence : ResolvePlacePresence {
		readonly string allOptions;
		public SpyOnPlacePresence( string allOptions, int focus, Track source )
			:base(allOptions.Split(';')[0],focus,source)
		{
			this.allOptions = allOptions;
		}
		protected override void Update(PlacePresenceBase pp ) {

			base.Update( pp );

			string actualOptions = pp.Options
				.Select( o=> o.Text )
				.OrderBy(l=>l)
				.Join(";");

			if(actualOptions != allOptions)
				throw new Exception($"Expected [{allOptions}] but found [{actualOptions}]" );
		}
	}

}
