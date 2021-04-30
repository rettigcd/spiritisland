using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : GrowthAction {

		public RangeCriteria[] rc;

		#region constructors

		public PlacePresence(
			Spirit spirit,

			int range,
			IEnumerable<Space> referenceSpaces = null
		):base(spirit){
			rc = new RangeCriteria[]{ new RangeCriteria(range) };
			this.referenceSpaces = referenceSpaces ?? spirit.CanPlacePresenceFrom;
		}

		public PlacePresence(
			Spirit spirit,
			IEnumerable<Space> referenceSpaces,
			params RangeCriteria[] rc
		):base(spirit){
			this.rc = rc;
			this.referenceSpaces = referenceSpaces ?? spirit.CanPlacePresenceFrom;
		}

		public PlacePresence(
			Spirit spirit,
			params RangeCriteria[] rc
		):base(spirit){
			this.rc = rc;
			this.referenceSpaces = spirit.CanPlacePresenceFrom;
		}


		#endregion

		public override void Apply() {

			Space[][] options = this.PresenseToPlaceOptions();
			var optionStrings = options
				.Select( o => string.Join( "", o.Select( bs => bs.Label ).OrderBy( l => l ) ) )
				.OrderBy( s => s )
				.ToArray();

			if(!optionStrings.Contains(placeOnSpace))
				throw new InvalidPresenceLocation(placeOnSpace,optionStrings);

			placeOnSpace = null;
		}

		public Space[][] PresenseToPlaceOptions(){
			return PresenceCalculator.PresenseToPlaceOptions(referenceSpaces, this.rc );
		} 


		string placeOnSpace;
		readonly IEnumerable<Space> referenceSpaces;

		static public IResolver Place(string placeOnSpace) => new Resolve(placeOnSpace);

		class Resolve : IResolver {

			readonly string placeOnSpace;

			public Resolve(string placeOnSpace){
				this.placeOnSpace = placeOnSpace;
			}

			public void Apply( GrowthOption growthActions ) {
				var pp = growthActions.GrowthActions
					.OfType<PlacePresence>()
//					.Cast<IPresenceCriteria>()
					.ToArray();
				if(pp.Length>1) throw new InvalidOperationException("combine presence");
				pp[0].placeOnSpace = this.placeOnSpace;
			}
		}

	}


}
