using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Tests {

	public class ResolvePlacePresence {

		readonly string allOptions;
		readonly string placeOnSpace;
		readonly Track source;
		readonly string factoryDescription;

		public ResolvePlacePresence(string allOptions, Track source,string factoryDescription) {
			this.allOptions = allOptions;
			this.placeOnSpace = allOptions.Split(';')[0];
			this.source = source;
			this.factoryDescription = factoryDescription;
		}

		public void Apply(Spirit spirit,GameState gameState) {

			IActionFactory factory = (factoryDescription == null)
				? FindSinglePlacePresence( spirit, gameState )
				: FindMatchingName( spirit );

			var action = factory.Bind( spirit, gameState );
			if(action is not PlacePresenceBaseAction pp) throw new Exception( "expected PlacePresence but found " + action.GetType() );

			pp.Select( source );

			if(allOptions.Length>2){
				// !!! validate options here

				pp.Select( pp.Options.Single( o => o.Text == placeOnSpace ) );
			}

			pp.Apply();
			spirit.Resolve( factory );
		}

		private IActionFactory FindMatchingName( Spirit spirit ) {
			return spirit.UnresolvedActionFactories
				.FirstOrDefault( f => f.Name == factoryDescription )
				?? throw new Exception( "Could not find factory [" + factoryDescription + "] in " + spirit.UnresolvedActionFactories.Select( f => f.Name ).Join( ", " ) );
		}

		static IActionFactory FindSinglePlacePresence( Spirit spirit, GameState gameState ) {
			var ppFactories = spirit.UnresolvedActionFactories
				.Where( f => f.Bind( spirit, gameState ) is PlacePresenceBaseAction )
				.ToArray();

			if(ppFactories.Length == 1) return ppFactories[0];
			if(ppFactories.Length == 0) throw new Exception("no PP factories found");

			if(ppFactories.Select(x=>x.Name).Distinct().Count()==1)
				return ppFactories[0]; // same name , just use the first one

			throw new Exception("multiple PP factories found. Please select from: "+ppFactories.Select(f=>f.Name).Join(", "));

		}

	}


}
