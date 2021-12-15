using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SeekSafety : IFearOptions {

		public const string Name = "Seek Safety";

		[FearLevel( 1, "Each player may Push 1 Explorer into a land with more Town / City than the land it came from." )]
		public async Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;

			var buildingCounts = gs.Island.AllSpaces.ToDictionary(s=>s,s=>gs.Tokens[s].TownsAndCitiesCount());
			Space[] GetNeighborWithMoreBuildings( Space s ) => s.Adjacent.Where( n => buildingCounts[n] > buildingCounts[s] ).ToArray();
			bool HasNeighborWithMoreBuildings(Space s) => GetNeighborWithMoreBuildings(s).Any();

			// Each player may
			foreach(var spiritCtx in ctx.Spirits) {

				// Push 1 Explorer into a land with more Town / City than the land it came from.

				// Select Source
				var sourceOptions = gs.Island.AllSpaces
					.Where(s=>gs.Tokens[s].Has(Invader.Explorer) && HasNeighborWithMoreBuildings(s))
					.ToArray();
				if(sourceOptions.Length==0) return;
				var source = await spiritCtx.Self.Action.Decision( new Decision.TargetSpace( "Fear: Select land to push explorer from into more towns/cities", sourceOptions,Present.Done));
				if(source==null) continue; // continue => next spirit, break/return => no more spirits

				// Select Destination
				var destinationOptions = GetNeighborWithMoreBuildings(source);
				var dest = await spiritCtx.Self.Action.Decision( new Decision.AdjacentSpace( "Fear: select destination with more towns/cities", source,Decision.AdjacentDirection.Outgoing, destinationOptions, Present.Always));

				// push
				await spiritCtx.Move(Invader.Explorer[1],source,dest);
			}
		}

		[FearLevel( 2, "Each player may Gather 1 Explorer into a land with Town / City, or Gather 1 Town into a land with City." )]
		public async Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(var spiritCtx in ctx.Spirits) {
				var options = gs.Island.AllSpaces
					.Where( s => gs.Tokens[ s ].HasAny(Invader.Town,Invader.City) )
					.ToArray();
				if(options.Length == 0) break;
				var dest = await spiritCtx.Self.Action.Decision( new Decision.TargetSpace( "Select space to gather town to city OR explorer to town", options, Present.Always));
				var destCtx = spiritCtx.Target(dest);
				var grp = destCtx.Tokens;
				var invadersToGather = new List<TokenGroup>();
				if(grp.Has(Invader.City)) invadersToGather.Add( Invader.Town );
				if(grp.Has(Invader.Town)) invadersToGather.Add( Invader.Explorer );
				TokenGroup[] invadersToGatherArray = invadersToGather.ToArray();

				await destCtx.GatherUpTo(1, invadersToGatherArray);
			}
		}

		[FearLevel( 3, "Each player may remove up to 3 Health worth of Invaders from a land without Town." )]
		public async Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			foreach(SelfCtx spirit in ctx.Spirits) {

				var options = gs.Island.AllSpaces
					.Where( s => {var counts = gs.Tokens[ s ]; return counts.HasInvaders() && !counts.Has(Invader.Town); } )
					.ToArray();
				if(options.Length==0) return;
				var target = await spirit.Self.Action.Decision( new Decision.TargetSpace( "Select space to remove 3 health of invaders", options, Present.Always));
				var sCtx = spirit.Target(target);

				if(sCtx.Tokens.Has(Invader.City))
					sCtx.RemoveInvader(Invader.City);
				else
					sCtx.Tokens.Adjust(Invader.Explorer[1],-3);
			}
		}

	}
}
