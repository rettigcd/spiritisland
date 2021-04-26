using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {


	public interface IPresenceCriteria{
		int Range {get; }
		bool IsValid(Space bs,GameState gs);
	}

	class PresenceCalculator {

		static public Space[][] PresenseToPlaceOptions(PlayerState ps,GameState gs){
			var calc = new PresenceCalculator( ps.Presence, gs );
			calc.Execute(ps.PresenceToPlace.ToArray());
			if(ps.PresenceToPlace.Count == 2)
				calc.Execute(ps.PresenceToPlace[1],ps.PresenceToPlace[0]);
			return calc.Results;
		}

		readonly GameState gameState;

		public PresenceCalculator(List<Space> existingPresense,GameState gs){
			this.existingPresence = existingPresense.ToArray();
			this.gameState = gs;
		}

		public void Execute( params IPresenceCriteria[] criteria ){
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
				.Where( bs=>criterion.IsValid(bs,gameState) )
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

		IPresenceCriteria[] criteria;
		Space[] xx;
		readonly Space[] existingPresence;

		readonly HashSet<Space[]> results = new HashSet<Space[]>();
		readonly HashSet<string> keys = new HashSet<string>(); 
	}


}
