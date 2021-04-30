using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class RangeCriteria : IPresenceCriteria {
		public RangeCriteria(int range,Func<Space,GameState,bool> criteria=null){
			this.Range = range;
			this.Criteria = criteria ?? ((s,gs)=>true);
		}
		public int Range {get;}
		public Func<Space,GameState,bool> Criteria {get;}

		public bool IsValid( Space bs, GameState gs ) => Criteria(bs,gs);
	}

	public class PlacePresence : GrowthAction {

		public RangeCriteria rc;
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
			rc = new RangeCriteria(range,criteria);
			// this is for Ocean
			this.referenceSpaces = referenceSpaces ?? spirit.CanPlacePresenceFrom;
		}

		#endregion

		public override void Apply() {
			spirit.PresenceToPlace.Add( this.rc );

// if there is only 1 to place
			if(placeOnSpace != null && !placeOnSpace.Contains(";")){
				var candidates = referenceSpaces
					.SelectMany(s=>s.SpacesWithin(rc.Range))
					.Distinct()
					.ToArray();
//!!! need to test critera filter also

				if(!candidates.Select(x=>x.Label).Contains(placeOnSpace))
					throw new InvalidPresenceLocation();
			}
			placeOnSpace = null;
		}

		public Space[][] PresenseToPlaceOptions(){
			var list = new List<IPresenceCriteria> { this.rc };
			return PresenceCalculator.PresenseToPlaceOptions(referenceSpaces, list, gs );
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

}
