using SpiritIsland.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SeekSafety : IFearCard {

		[FearLevel( 1, "Each player may Push 1 Explorer into a land with more Town / City than the land it came from." )]
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

		[FearLevel( 2, "Each player may Gather 1 Explorer into a land with Town / City, or Gather 1 Town into a land with City." )]
		public async Task Level2( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine( spirit, gs );
				var options = gs.Island.AllSpaces
					.Where( s => gs.InvadersOn( s ).Has(Invader.Town,Invader.City) )
					.ToArray();
				var dest = await engine.SelectSpace("Select space to gather town to city OR explorer to town",options);
				var grp = gs.InvadersOn(dest);
				var invadersToGather = new List<Invader>();
				if(grp.HasCity) invadersToGather.Add( Invader.Town );
				if(grp.HasTown) invadersToGather.Add( Invader.Explorer );
				var x = invadersToGather.ToArray();
				var sourceOptions = dest.Neighbors.Where(s=>gs.InvadersOn(s).Has(x)).ToArray();
				if(sourceOptions.Length==0) continue;
				var source = await engine.SelectSpace("Select source of invaders to gather",sourceOptions);
				var invaderOptions = gs.InvadersOn(source).InvaderTypesPresent.Intersect(x).ToArray();
				var invaderToGather = await engine.SelectInvader("which invader to gather",invaderOptions);
				gs.Adjust(source,invaderToGather,-1);
				gs.Adjust( dest, invaderToGather, 1 );
			}
		}

		[FearLevel( 3, "Each player may remove up to 3 Health worth of Invaders from a land without Town." )]
		public async Task Level3( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var engine = new ActionEngine( spirit, gs );
				var options = gs.Island.AllSpaces
					.Where( s => {var grp = gs.InvadersOn( s ); return grp.TotalCount>0 && !grp.HasTown; } )
					.ToArray();
				if(options.Length==0) return;
				var target = await engine.SelectSpace("Select space to remove 3 health of invaders",options);
				var grp = gs.InvadersOn(target);
				if(grp.HasCity)
					gs.Adjust(target,Invader.City,-1);
				else
					gs.Adjust(target,Invader.Explorer,Math.Min(3,grp[Invader.Explorer]));
			}
		}

	}
}
