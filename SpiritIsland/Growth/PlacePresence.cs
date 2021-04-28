using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PlacePresence : GrowthAction, IPresenceCriteria {

		#region constructors

		public PlacePresence(int range):this( range, (s,gs)=>true ){}

		public PlacePresence(int range,Func<Space,GameState,bool> criteria){ 
			this.Range = range;
			this.criteria = criteria;
		}

		#endregion

		public int Range {get;}

		public bool IsValid(Space bs, GameState gs) => criteria(bs,gs);

		public override void Apply( Spirit ps ) {
			ps.PresenceToPlace.Add( this );
		}

		readonly Func<Space,GameState,bool> criteria;


		class Resolve : IResolver {
			public void Apply( GrowthOption growthActions ) {
				var pp = growthActions.GrowthActions.OfType<PlacePresence>().ToArray();
//				throw new NotImplementedException();
			}
		}

	}

}
