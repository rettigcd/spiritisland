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
			RangeCriteria rc
		):base(spirit){
			this.rc = new RangeCriteria[]{ rc };
			this.referenceSpaces = referenceSpaces ?? spirit.Presence;
		}

		public PlacePresence(
			Spirit spirit,
			RangeCriteria rc
		):base(spirit){
			this.rc = new RangeCriteria[]{ rc };
			this.referenceSpaces = spirit.Presence;
		}


		#endregion

		public override void Apply() {

			static string FormatOption(Space bs) => bs.Label;

			var opts = Options;
			var option = Options
				.FirstOrDefault(o => FormatOption(o) == placeOnSpace);

			if( option == null )
				throw new InvalidPresenceLocation(placeOnSpace,Options.Select(FormatOption).ToArray());

			this.spirit.Presence.Add(option);

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

		public Space[] Options => options ??= CalculateOptions();
		Space[] options;
		Space[]  CalculateOptions() => PresenceCalculator.PresenseToPlaceOptions(
			referenceSpaces, 
			this.rc[0]
		);

		string placeOnSpace;
		readonly IEnumerable<Space> referenceSpaces;
		Track[] source;

		static public IResolver Place(string placeOnSpace, int focus, Track source) 
			=> new Resolve(placeOnSpace,focus,source);

		public class Resolve : IResolver {

			readonly string placeOnSpace;
			readonly Track[] source;
			readonly int focus;

			public Resolve(string placeOnSpace,int focus, params Track[] source){
				this.placeOnSpace = placeOnSpace;
				this.source = source;
				this.focus = focus;
			}

			public void Apply( GrowthOption growthActions ) {
				var placePresence = growthActions.GrowthActions
					.OfType<PlacePresence>()
					.ToArray()[focus];
				Update( placePresence );
			}

			protected virtual void Update(PlacePresence pp) {
				pp.placeOnSpace = placeOnSpace;
				pp.source = source;
			}
		}

	}


}
