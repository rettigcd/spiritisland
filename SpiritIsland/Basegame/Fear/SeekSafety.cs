using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	class SeekSafety : IFearCard {

		//"Each player may Push 1 Explorer into a land with more Town / City than the land it came from.", 
		public async Task Level1( GameState gs ) {
			var buildingCounts = gs.Island.AllSpaces.ToDictionary(s=>s,s=>gs.InvadersOn(s).TypeCount(Invader.City,Invader.Town));
			Space[] GetNeighborWithMoreBuildings( Space s ) => s.Neighbors.Where( n => buildingCounts[n] > buildingCounts[s] ).ToArray();
			bool HasNeighborWithMoreBuildings(Space s) => GetNeighborWithMoreBuildings(s).Any();
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine(spirit,gs);
				var options = gs.Island.AllSpaces
					.Where(s=>gs.InvadersOn(s).HasExplorer && HasNeighborWithMoreBuildings(s))
					.ToArray();
				if(options.Length==0) return;
				var target = await engine.SelectSpace("Fear: Select land to push explorer from into more towns/cities",options,true);
				if(target==null) continue; // continue => next spirit, break/return => no more spirits
				var destinations = GetNeighborWithMoreBuildings(target);
				var dest = await engine.SelectSpace("Fear: select destination with more towns/cities",destinations);
				// push
				gs.Adjust(target,Invader.Explorer,-1);
				gs.Adjust( dest, Invader.Explorer, 1 );
			}
		}

		//"Each player may Gather 1 Explorer into a land with Town / City, or Gather 1 Town into a land with City.", 
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		//"Each player may remove up to 3 Health worth of Invaders from a land without Town."),
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}

	}
}
