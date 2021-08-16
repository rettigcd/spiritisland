﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WrapInWingsOfSunlight {

		[MajorCard( "Wrap in Wings of Sunlight", 3, Speed.Fast, Element.Sun, Element.Air, Element.Animal )]
		[FromPresence(0)]
		static public async Task ActAsync(ActionEngine engine,Space target ) {

			// if you have 2 sun, 2 air, 2 animal, // First Gather up to 3 dahan
			if(engine.Self.Elements.Contains( "2 sun,2 air,2 animal" ))
				await engine.GatherUpToNDahan( target, 3 );

			// move up to 5 dahan from target land to any land.
			// defend 5 in that land

			// destination
			var destination = await engine.SelectSpace( "Move dahan to", engine.GameState.Island.AllSpaces.Where( s => s.Terrain != Terrain.Ocean ) );

			// move dahan
			int max = Math.Min( engine.GameState.GetDahanOnSpace( target ), 5 );
			int countToMove = await SelectNumber( engine, "# of dahan to move", target, destination, max );
			await engine.GameState.MoveDahan( target, destination, countToMove );

			// defend
			engine.GameState.Defend( destination, 5 );

		}

		private static async Task<int> SelectNumber( ActionEngine engine, string prompt, Space target, Space destination, int count ) {
			List<string> numToMove = new List<string>();
			while(count > 0) numToMove.Add( (count--).ToString() );
			return int.Parse( await engine.SelectText( prompt, numToMove.ToArray() ) );
		}
	}

}
