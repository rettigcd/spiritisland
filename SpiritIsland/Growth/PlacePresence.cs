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
			this.referenceSpaces = referenceSpaces ?? spirit.Presence;
		}

		public PlacePresence(
			Spirit spirit,
			IEnumerable<Space> referenceSpaces,
			params RangeCriteria[] rc
		):base(spirit){
			this.rc = rc;
			this.referenceSpaces = referenceSpaces ?? spirit.Presence;
		}

		public PlacePresence(
			Spirit spirit,
			params RangeCriteria[] rc
		):base(spirit){
			this.rc = rc;
			this.referenceSpaces = spirit.Presence;
		}


		#endregion

		public override void Apply() {

			string FormatOption(Space[] o) => o.Select( bs => bs.Label ).OrderBy( l => l ).Join("");

			var option = this.Options
				.Where(o => FormatOption(o) == placeOnSpace)
				.FirstOrDefault();

			if( option == null )
				throw new InvalidPresenceLocation(placeOnSpace,Options.Select(FormatOption).ToArray());

			foreach(var space in option)
				this.spirit.Presence.Add(space);

			placeOnSpace = null;
		}

		public Space[][] Options => options ?? (options=CalculateOptions());
		Space[][] options;
		Space[][]  CalculateOptions() => PresenceCalculator.PresenseToPlaceOptions(referenceSpaces, this.rc );


		string placeOnSpace;
		readonly IEnumerable<Space> referenceSpaces;

		static public IResolver Place(string placeOnSpace) => new Resolve(placeOnSpace);

		public class Resolve : IResolver {

			readonly string placeOnSpace;

			public Resolve(string placeOnSpace){
				this.placeOnSpace = placeOnSpace;
			}

			public void Apply( GrowthOption growthActions ) {
				var pp = growthActions.GrowthActions
					.OfType<PlacePresence>()
					.ToArray();
				switch(pp.Length) {
					case 0: throw new InvalidOperationException( "no place precense action available" );
					case 1: Update(pp[0]); break;
					default: throw new InvalidOperationException( "combine presence" );
				}
			}

			protected virtual void Update(PlacePresence pp) {
				pp.placeOnSpace = this.placeOnSpace;
			}
		}

	}


}
