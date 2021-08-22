using SpiritIsland;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	// ! this could split up into 3 Extension files
	// * GatherExtensions
	// * PushExtesnsions
	// * PlacePresenceExtensions

	public interface IMakeGamestateDecisions {
		Spirit Self { get; }
		GameState GameState { get; }
	}

	public static class IMakeGamestateDecisionsExtensions {

		static public IMakeGamestateDecisions MakeDecisionsFor(this Spirit spirit, GameState gameState ) => new GsDecisionMaker(spirit,gameState);

		class GsDecisionMaker : IMakeGamestateDecisions {
			public GsDecisionMaker(Spirit spirit,GameState gs ) { this.Self = spirit; GameState = gs; }
			public Spirit Self { get; }

			public GameState GameState { get; }
		}

		#region Gather

		static public async Task GatherUpToNDahan( this IMakeGamestateDecisions eng, Space target, int dahanToGather ) {
  			int gathered = 0;
			var neighborsWithDahan = target.Adjacent.Where(eng.GameState.HasDahan).ToArray();
			while(gathered<dahanToGather && neighborsWithDahan.Length>0){
				var source = await eng.Self.SelectSpace( $"Gather dahan {gathered+1} of {dahanToGather} from:", neighborsWithDahan, Present.Done);
				if(source == null) break;

				await eng.GameState.MoveDahan(source,target);

				++gathered;
				neighborsWithDahan = target.Adjacent.Where(eng.GameState.HasDahan).ToArray();
			}

		}

		static public async Task GatherUpToNInvaders( this IMakeGamestateDecisions eng, Space target, int countToGather, params Invader[] ofType ) {
			InvaderSpecific[] spaceInvaders(Space space) => eng.GameState.InvadersOn(space).FilterBy(ofType);
			Space[] CalcSource() => target.Adjacent
				.Where(s=>spaceInvaders(s).Any())
				.ToArray();

			string label = ofType.Select(it=>it.Label).Join("/");

			Space[] neighborsWithItems = CalcSource();
  			int gathered = 0;
			while(gathered<countToGather && neighborsWithItems.Length>0){
				var source = await eng.Self.SelectSpace( $"Gather {label} {gathered+1} of {countToGather} from:", neighborsWithItems, Present.Done);
				if(source == null) break;

				var invader = await eng.Self.SelectInvader("Select invader to gather "+source.Label+" => "+target.Label,spaceInvaders(source));

				await eng.GameState.MoveInvader(invader, source, target);

				++gathered;
				neighborsWithItems = CalcSource();
			}

		}

		#endregion Gather

		#region Push

		static public async Task<Space[]> FearPushUpToNDahan( this IMakeGamestateDecisions eng, Space source, int dahanToPush) {
			HashSet<Space> pushedToLands = new HashSet<Space>();
			dahanToPush = System.Math.Min(dahanToPush,eng.GameState.DahanCount(source));
			while(0<dahanToPush){
				Space destination = await eng.Self.SelectSpace("Select destination for dahan"
					,source.Adjacent.Where(n=>n.Terrain != Terrain.Ocean ) // This is non-power push, so directly checking ocean is ok
					,Present.Done
				);
				if(destination == null) break;
				pushedToLands.Add(destination);
				await eng.GameState.MoveDahan(source,destination);
				--dahanToPush;
			}
			return pushedToLands.ToArray();
		}

		// non-power push (for fear)
		static public async Task FearPushUpToNInvaders( this IMakeGamestateDecisions eng, Space source, int countToPush
			,params Invader[] healthyInvaders
		) {

			InvaderSpecific[] CalcInvaderTypes() => eng.GameState.InvadersOn(source).FilterBy(healthyInvaders);

			var invaders = CalcInvaderTypes();
			while(0<countToPush && 0<invaders.Length){
				var invader = await eng.Self.SelectInvader("Select invader to push",invaders, Present.Done );
				if(invader==null) 
					break;

				var destination = await eng.Self.SelectSpace( "Push " + invader.Summary + " to", source.Adjacent.Where( x => x.Terrain != Terrain.Ocean ) ); 
				await eng.GameState.MoveInvader(invader, source, destination );

				--countToPush;
				invaders = CalcInvaderTypes();
			}
		}

		#endregion Push

		#region Place Presence

		static public Task PlacePresence( this IMakeGamestateDecisions engine, int range, Target filterEnum ) {
			Space[] destinationOptions = engine.Self.Presence.Spaces
				.SelectMany( s => s.Range( range ) )
				.Distinct()
				.Where( SpaceFilter.ForPlacingPresence.GetFilter( engine.Self, engine.GameState, filterEnum ) )
				.OrderBy( x => x.Label )
				.ToArray();
			return engine.PlacePresence(destinationOptions);
		}

		static public async Task PlacePresence( this IMakeGamestateDecisions engine, params Space[] destinationOptions ) {

			var from = await engine.Self.SelectTrack();

			var to = await engine.Self.SelectSpace( "Where would you like to place your presence?", destinationOptions );
			engine.Self.Presence.PlaceFromBoard( from, to );

		}

		#endregion Place Presence


		/// <summary>
		/// Used for Power-targetting, where range sympols appear.
		/// </summary>
		static public Task<Space> PowerTargetsSpace( this IMakeGamestateDecisions engine, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum )
			=> engine.Self.PowerApi.TargetsSpace( engine.Self, engine.GameState, sourceEnum, sourceTerrain, range, filterEnum );

		// Not Changable!
		static public InvaderGroup InvadersOn( this IMakeGamestateDecisions engine, Space space )
			=> engine.Self.BuildInvaderGroup( engine.GameState, space );

		static public async Task DamageInvaders( this IMakeGamestateDecisions engine, Space space, int damage ) { // !!! let players choose the item to apply damage to
			if(damage == 0) return;
			await engine.InvadersOn( space ).ApplySmartDamageToGroup( damage );
		}

	}

}