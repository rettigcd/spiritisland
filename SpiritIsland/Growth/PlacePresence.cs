using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : GrowthAction, IPresenceCriteria {

		#region constructors

		public PlacePresence(Spirit spirit, int range):this( spirit, range, (s,gs)=>true ){}

		public PlacePresence(Spirit spirit, int range,Func<Space,GameState,bool> criteria)
			:base(spirit)
		{ 
			this.Range = range;
			this.criteria = criteria;
		}

		#endregion

		public int Range {get;}

		public bool IsValid(Space bs, GameState gs) => criteria(bs,gs);

		public override void Apply() {
			spirit.PresenceToPlace.Add( this );

// if there is only 1 to place
			if(placeOnSpace != null && !placeOnSpace.Contains(";")){
				var candidates = spirit.CanPlacePresenceFrom
					.SelectMany(s=>s.SpacesWithin(Range))
					.Distinct()
					.ToArray();
//!!! need to test critera filter also

				if(!candidates.Select(x=>x.Label).Contains(placeOnSpace))
					throw new InvalidPresenceLocation();
			}
			placeOnSpace = null;
		}

		readonly Func<Space,GameState,bool> criteria;

		string placeOnSpace;

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
				if(pp.Length == 1){
					pp[0].placeOnSpace = this.placeOnSpace;
				}
			}
		}

	}

}
