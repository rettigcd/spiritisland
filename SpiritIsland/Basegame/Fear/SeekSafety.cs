using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SeekSafety : IFearCard {

		public const string Name = "Seek Safety";

		[FearLevel( 1, "Each player may Push 1 Explorer into a land with more Town / City than the land it came from." )]
		public async Task Level1( GameState gs ) {
			var buildingCounts = gs.Island.AllSpaces.ToDictionary(s=>s,s=>gs.InvadersOn(s).TownsAndCitiesCount);
			Space[] GetNeighborWithMoreBuildings( Space s ) => s.Adjacent.Where( n => buildingCounts[n] > buildingCounts[s] ).ToArray();
			bool HasNeighborWithMoreBuildings(Space s) => GetNeighborWithMoreBuildings(s).Any();
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces
					.Where(s=>gs.InvadersOn(s).HasExplorer && HasNeighborWithMoreBuildings(s))
					.ToArray();
				if(options.Length==0) return;
				var target = await spirit.SelectSpace("Fear: Select land to push explorer from into more towns/cities",options,Present.Done);
				if(target==null) continue; // continue => next spirit, break/return => no more spirits
				var destinations = GetNeighborWithMoreBuildings(target);
				var dest = await spirit.SelectSpace("Fear: select destination with more towns/cities",destinations);
				// push
				await gs.MoveInvader(InvaderSpecific.Explorer,target,dest);
			}
		}

		[FearLevel( 2, "Each player may Gather 1 Explorer into a land with Town / City, or Gather 1 Town into a land with City." )]
		public async Task Level2( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces
					.Where( s => gs.InvadersOn( s ).HasAny(Invader.Town,Invader.City) )
					.ToArray();
				var dest = await spirit.SelectSpace("Select space to gather town to city OR explorer to town",options);
				var grp = gs.InvadersOn(dest);
				var invadersToGather = new List<Invader>();
				if(grp.Has(Invader.City)) invadersToGather.Add( Invader.Town );
				if(grp.Has(Invader.Town)) invadersToGather.Add( Invader.Explorer );
				Invader[] invadersToGatherArray = invadersToGather.ToArray();
				var sourceOptions = dest.Adjacent.Where(s=>gs.InvadersOn(s).HasAny(invadersToGatherArray)).ToArray();
				if(sourceOptions.Length==0) continue;
				var source = await spirit.SelectSpace("Select source of invaders to gather",sourceOptions);

				var invaderOptions = gs.InvadersOn( source ).InvaderTypesPresent_Specific
					.Where(specific => invadersToGatherArray.Contains(specific.Generic) )
					.ToArray();

				var invaderToGather = await spirit.SelectInvader(source,"which invader to gather",invaderOptions);
				await gs.MoveInvader(invaderToGather,source,dest);
			}
		}

		[FearLevel( 3, "Each player may remove up to 3 Health worth of Invaders from a land without Town." )]
		public async Task Level3( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces
					.Where( s => {var grp = gs.InvadersOn( s ); return grp.TotalCount>0 && !grp.HasTown; } )
					.ToArray();
				if(options.Length==0) return;
				var target = await spirit.SelectSpace("Select space to remove 3 health of invaders",options);
				var grp = gs.InvadersOn(target);
				if(grp.HasCity)
					gs.Adjust(target,InvaderSpecific.City,-1);
				else
					gs.Adjust(target,InvaderSpecific.Explorer,-3);
			}
		}

	}
}
