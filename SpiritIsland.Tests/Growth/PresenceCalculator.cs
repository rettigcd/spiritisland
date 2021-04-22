using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	class PresenceCalculator {
		public PresenceCalculator(List<BoardSpace> existingPresense, Func<BoardSpace,bool> filter){
			this.existingPresence = existingPresense.ToArray();
			this.filter = filter;
		}

		public void Execute( params int[] jumps ){
			this.jumps = jumps;
			this.xx = new BoardSpace[jumps.Length];
			FindPresence( 0 );
		}

		public BoardSpace[][] Results => results.ToArray(); 

		void FindPresence( int index ) {
			if(index == jumps.Length){
				Add();
				return;
			}

			var options = existingPresence.Union(xx.Take(index))
				.SelectMany( p => p.SpacesWithin( jumps[index] ) )
				.Distinct()
				.Where( bs => bs.Terrain != Terrain.Ocean )
				.Where( filter )
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


		int[] jumps;
		BoardSpace[] xx;
		readonly BoardSpace[] existingPresence;
		readonly Func<BoardSpace,bool> filter;

		readonly HashSet<BoardSpace[]> results = new HashSet<BoardSpace[]>();
		readonly HashSet<string> keys = new HashSet<string>(); 
	}


}
