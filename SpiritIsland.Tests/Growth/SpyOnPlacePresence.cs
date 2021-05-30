using System;
using System.Linq;

namespace SpiritIsland.Tests.Growth {

	public class SpyOnPlacePresence : PlacePresence.Resolve {
		readonly string allOptions;
		public SpyOnPlacePresence( string allOptions, int focus, params Track[] source )
			:base(allOptions.Split(';')[0],focus,source)
		{
			this.allOptions = allOptions;
		}
		protected override void Update( PlacePresence pp ) {

			base.Update( pp );

			string actualOptions = pp.Options
				.Select( o=> o.Label )
				.OrderBy(l=>l)
				.Join(";");

			if(actualOptions != allOptions)
				throw new Exception($"Expected [{allOptions}] but found [{actualOptions}]" );
		}
	}

}
