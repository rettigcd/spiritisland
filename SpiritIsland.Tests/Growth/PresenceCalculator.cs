using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {


	public class PresenceCriteria{
		public int Range {get; set; }
		public Func<BoardSpace,bool> IsValid { get; set; }
	}

	class PresenceCalculator {

		static public BoardSpace[][] PresenseToPlaceOptions(PlayerState ps){
			var calc = new PresenceCalculator( ps.Presence );
			calc.Execute(ps.PresenceToPlace.ToArray());
			if(ps.PresenceToPlace.Count == 2)
				calc.Execute(ps.PresenceToPlace[1],ps.PresenceToPlace[0]);
			return calc.Results;
		}

		public PresenceCalculator(List<BoardSpace> existingPresense){
			this.existingPresence = existingPresense.ToArray();
		}

		public void Execute( params PresenceCriteria[] criteria ){
			this.criteria = criteria;
			this.xx = new BoardSpace[criteria.Length];
			FindPresence( 0 );
		}

		public BoardSpace[][] Results => results.ToArray(); 

		void FindPresence( int index ) {
			if(index == criteria.Length){
				Add();
				return;
			}

			var criterion = criteria[index];
			var options = existingPresence.Union(xx.Take(index))
				.SelectMany( p => p.SpacesWithin( criterion.Range ) )
				.Distinct()
				.Where( bs => bs.Terrain != Terrain.Ocean )
				.Where( criterion.IsValid )
				.ToList();
			
			foreach(var option in options){
				xx[index] = option;
				FindPresence(index+1);
			}
		}


		void Add() {
			BoardSpace[] opt = xx.OrderBy( x=> x.Label ).ToArray();
			string key = string.Join("",opt.Select(x=>x.Label));
			if(!keys.Contains( key )) {
				results.Add( opt );
				keys.Add( key );
			}
		}

		PresenceCriteria[] criteria;
		BoardSpace[] xx;
		readonly BoardSpace[] existingPresence;

		readonly HashSet<BoardSpace[]> results = new HashSet<BoardSpace[]>();
		readonly HashSet<string> keys = new HashSet<string>(); 
	}


}
