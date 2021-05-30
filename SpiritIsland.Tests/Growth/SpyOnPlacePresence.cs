using System;
using System.Linq;

namespace SpiritIsland.Tests.Growth {

	public class SpyOnPlacePresence : PlacePresence.Resolve {
		readonly string allOptions;
		public SpyOnPlacePresence( string allOptions, params Track[] source )
			:base(allOptions.Split(';')[0].Substring(0,2),source)
		{
			this.allOptions = allOptions
				.Split(';')
				.Select(s=>s.Substring(0,2))
				.Distinct()
				.Join(";");
		}
		protected override void Update( PlacePresence pp ) {

			base.Update( pp );

			string actualOptions = pp.Options
				.Select( o=> o[0].Label )//.Select(l=>l.Label.Substring(0,2)).Join() )
				.Distinct()
				.OrderBy(l=>l)
				.Join(";");

			if(actualOptions != allOptions)
				throw new Exception($"Expected [{allOptions}] but found [{actualOptions}]" );
		}
	}

}
