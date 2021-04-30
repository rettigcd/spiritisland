using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : GrowthAction {

		public IPresenceCriteria[] rc;
		readonly GameState gs;

		#region constructors

		public PlacePresence(
			Spirit spirit,
			GameState gs,

			int range,
			Func<Space,GameState,bool> criteria = null,

			IEnumerable<Space> referenceSpaces = null
		):base(spirit){
			this.gs = gs;
			rc = new IPresenceCriteria[]{ new RangeCriteria(range,criteria) };
			// this is for Ocean
			this.referenceSpaces = referenceSpaces ?? spirit.CanPlacePresenceFrom;
		}

		#endregion

		public override void Apply() {
			spirit.PresenceToPlace.Add( this.rc[0] );

// if there is only 1 to place
			if(placeOnSpace != null){
//				if( !placeOnSpace.Contains(";")){
//					var candidates = referenceSpaces
//						.SelectMany(s=>s.SpacesWithin(rc[0].Range))
//						.Distinct()
//						.ToArray();
	//!!! need to test critera filter also
//					var	xx = candidates.Select(x=>x.Label).ToArray();

//					if(!xx.Contains(placeOnSpace))
//						throw new InvalidPresenceLocation(placeOnSpace,xx);
//				} else {
					Space[][] options = this.PresenseToPlaceOptions();
					var optionStrings = options
						.Select( o => string.Join( "", o.Select( bs => bs.Label ).OrderBy( l => l ) ) )
						.OrderBy( s => s )
						.ToArray();
					if(!optionStrings.Contains(placeOnSpace))
						throw new InvalidPresenceLocation(placeOnSpace,optionStrings);
//				}
				placeOnSpace = null;
			}
		}

		public Space[][] PresenseToPlaceOptions(){
			return PresenceCalculator.PresenseToPlaceOptions(referenceSpaces, this.rc, gs );
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
				if(pp.Length == 1){
					pp[0].placeOnSpace = this.placeOnSpace;
				}
			}
		}

	}

	public class RangeCriteria : IPresenceCriteria {
		public RangeCriteria(int range,Func<Space,GameState,bool> criteria=null){
			this.Range = range;
			this.Criteria = criteria ?? ((s,gs)=>true);
		}
		public int Range {get;}
		public Func<Space,GameState,bool> Criteria {get;}

		public bool IsValid( Space bs, GameState gs ) => Criteria(bs,gs);
	}


}
