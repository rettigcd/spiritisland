﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PresenceCalculator {

		static public Space[][] PresenseToPlaceOptions( 
			IEnumerable<Space> src, 
			RangeCriteria[] criteriaList
		){
			
			var calc = new PresenceCalculator( src );
			calc.Execute(criteriaList.ToArray());
			if(criteriaList.Length == 2)
				calc.Execute(criteriaList[1],criteriaList[0]);
			return calc.Results;
		}

		PresenceCalculator( IEnumerable<Space> existingPresense ){
			this.existingPresence = existingPresense.ToArray();
		}

		public void Execute( params RangeCriteria[] criteria ){
			this.criteria = criteria;
			this.xx = new Space[criteria.Length];
			FindPresence( 0 );
		}

		public Space[][] Results => results.ToArray(); 

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
				.Where( bs=>criterion.IsValid(bs) )
				.ToList();
			
			foreach(var option in options){
				xx[index] = option;
				FindPresence(index+1);
			}
		}


		void Add() {
			Space[] opt = xx.OrderBy( x=> x.Label ).ToArray();
			string key = string.Join("",opt.Select(x=>x.Label));
			if(!keys.Contains( key )) {
				results.Add( opt );
				keys.Add( key );
			}
		}

		RangeCriteria[] criteria;
		Space[] xx;
		readonly Space[] existingPresence;

		readonly HashSet<Space[]> results = new HashSet<Space[]>();
		readonly HashSet<string> keys = new HashSet<string>(); 
	}

}
