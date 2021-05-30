using System;
using System.Linq;

namespace SpiritIsland.Tests.Growth {

	public class SpyOnPlacePresence : PlacePresence.Resolve {
		readonly string allOptions;
		public SpyOnPlacePresence( string allOptions, params Track[] source )
			:base(allOptions.Split(';')[0],source)
		{
			this.allOptions = allOptions;
		}
		protected override void Update( PlacePresence pp ) {

			base.Update( pp );
			string[] x = pp.Options
				.Select(o=> o.Select(l=>l.Label)
				//	.OrderBy(l=>l)
					.Join() 
				)
				.Distinct()
				.OrderBy(l=>l)
				.ToArray();
			string actualOptions = x
				.Join(";");

			if(actualOptions != allOptions)
				throw new Exception($"Expected [{allOptions}] but found [{actualOptions}]" );
		}
	}

}
