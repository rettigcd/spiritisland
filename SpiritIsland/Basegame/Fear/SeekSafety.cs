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
			var buildingCounts = gs.Island.AllSpaces.ToDictionary(s=>s,s=>gs.Tokens[s].TownsAndCitiesCount());
			Space[] GetNeighborWithMoreBuildings( Space s ) => s.Adjacent.Where( n => buildingCounts[n] > buildingCounts[s] ).ToArray();
			bool HasNeighborWithMoreBuildings(Space s) => GetNeighborWithMoreBuildings(s).Any();
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces
					.Where(s=>gs.Tokens[s].Has(Invader.Explorer) && HasNeighborWithMoreBuildings(s))
					.ToArray();
				if(options.Length==0) return;
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Fear: Select land to push explorer from into more towns/cities", options,Present.Done));
				if(target==null) continue; // continue => next spirit, break/return => no more spirits
				var destinations = GetNeighborWithMoreBuildings(target);
				var dest = await spirit.Action.Choose( new TargetSpaceDecision( "Fear: select destination with more towns/cities", destinations));
				// push
				await gs.Move(Invader.Explorer[1],target,dest);
			}
		}

		[FearLevel( 2, "Each player may Gather 1 Explorer into a land with Town / City, or Gather 1 Town into a land with City." )]
		public async Task Level2( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces
					.Where( s => gs.Tokens[ s ].HasAny(Invader.Town,Invader.City) )
					.ToArray();
				var dest = await spirit.Action.Choose( new TargetSpaceDecision( "Select space to gather town to city OR explorer to town", options));
				var grp = gs.Tokens[dest];
				var invadersToGather = new List<TokenGroup>();
				if(grp.Has(Invader.City)) invadersToGather.Add( Invader.Town );
				if(grp.Has(Invader.Town)) invadersToGather.Add( Invader.Explorer );
				TokenGroup[] invadersToGatherArray = invadersToGather.ToArray();
				var sourceOptions = dest.Adjacent.Where(s=>gs.Tokens[s].HasAny(invadersToGatherArray)).ToArray();
				if(sourceOptions.Length==0) continue;
				var source = await spirit.Action.Choose( new GatherTokensFromDecision( 1, invadersToGatherArray, dest, sourceOptions ));

				var invaderOptions = gs.Tokens[source].Invaders()
					.Where(specific => invadersToGatherArray.Contains(specific.Generic) )
					.ToArray();

				var invaderToGather = await spirit.Action.Choose( new SelectTokenToGatherDecision( source, dest, invaderOptions, Present.IfMoreThan1 ) );
				
				await gs.Move( invaderToGather, source, dest);
			}
		}

		[FearLevel( 3, "Each player may remove up to 3 Health worth of Invaders from a land without Town." )]
		public async Task Level3( GameState gs ) {
			foreach(var spirit in gs.Spirits) {
				var options = gs.Island.AllSpaces
					.Where( s => {var counts = gs.Tokens[ s ]; return counts.HasInvaders() && !counts.Has(Invader.Town); } )
					.ToArray();
				if(options.Length==0) return;
				var target = await spirit.Action.Choose( new TargetSpaceDecision( "Select space to remove 3 health of invaders", options));
				var grp = gs.Tokens[target];
				if(grp.Has(Invader.City))
					grp.Remove(Invader.City);
				else
					grp.Adjust(Invader.Explorer[1],-3);
			}
		}

	}
}
