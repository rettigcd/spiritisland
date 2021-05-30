using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public enum Track { Energy, Card };

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

			static string FormatOption(Space[] o) => o.Select( bs => bs.Label ).OrderBy( l => l ).Join("");

			var option = Options
				.FirstOrDefault(o => FormatOption(o) == placeOnSpace);

			if( option == null )
				throw new InvalidPresenceLocation(placeOnSpace,Options.Select(FormatOption).ToArray());

			foreach(var space in option)
				this.spirit.Presence.Add(space);

			foreach(var src in source)
				switch(src){
					case Track.Card:
						spirit.RevealedCardSpaces++;
						break;
					case Track.Energy:
						spirit.RevealedEnergySpaces++;
						break;
				}

			placeOnSpace = null;
		}

		public Space[][] Options => options ??= CalculateOptions();
		Space[][] options;
		Space[][]  CalculateOptions() => PresenceCalculator.PresenseToPlaceOptions(referenceSpaces, this.rc );


		string placeOnSpace;
		readonly IEnumerable<Space> referenceSpaces;
		Track[] source;

		static public IResolver Place(string placeOnSpace,Track source) => new Resolve(placeOnSpace,source);

		public class Resolve : IResolver {

			readonly string placeOnSpace;
			readonly Track[] source;

			public Resolve(string placeOnSpace,params Track[] source){
				this.placeOnSpace = placeOnSpace;
				this.source = source;
			}

			public void Apply( GrowthOption growthActions ) {
				var pp = growthActions.GrowthActions
					.OfType<PlacePresence>()
					.ToArray();

				switch(pp.Length) {
					case 0: throw new InvalidOperationException( "no place presence action available" );
					case 1: Update(pp[0]); break;
					default: throw new InvalidOperationException( "combine presence" );
				}
			}

			protected virtual void Update(PlacePresence pp) {
				pp.placeOnSpace = placeOnSpace;
				pp.source = source;
			}
		}

	}


}
