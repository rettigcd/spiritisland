
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	/*
	====================================
	Oceans Hungry Grasp
	* reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
	* +1 presence range any ocean, +1 presense in any ociean, +1 energy
	* gain power card, push 1 presense from each ocian,  add presense on costal land range 1

	Special - Ocean in Play - You may add/move presnece into  oceans but may not add/move presense into inland lands.  On
	On boards with you presense, treat ocieans as coastal wetalnads for powers and blight.  YOu drown any invaters or dahan moved into those ocians
	Drowning - destorys drawned pieces, placing draowned invaters here.  At any time you may exchange X health of these invatores for 1 energy where x = number of players
	Setup - 1 in ocean and 1 in costal land of your choice.
	 */

	public class Ocean : Spirit {

		public const string Name = "Ocean's Hungry Grasp";

		public Ocean():base(
			new OceanPresence(
				new PresenceTrack( Track.Energy0, Track.MoonEnergy, Track.WaterEnergy, Track.Energy1, Track.EarthEnergy, Track.WaterEnergy, Track.Energy2 ),
				new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4, Track.Card5 )
			),
			PowerCard.For<CallOfTheDeeps>(),
			PowerCard.For<GraspingTide>(),
			PowerCard.For<SwallowTheLandDwellers>(),
			PowerCard.For<TidalBoon>()
		) {

			GrowthOptions = new GrowthOption[]{ 
				// Option 1 - reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
				new GrowthOption(
					new GatherPresenceIntoOcean(),
					new ReclaimAll(),
					new DrawPowerCard(),
					new GainEnergy(2)
				), 
				// Option 2 - +1 presence range any ocean, +1 presense in any ociean, +1 energy
				new GrowthOption(
					new GainEnergy(1),
					new PlaceInOcean(),
					new PlaceInOcean()
				), 
				// Option 3 - gain power card, push 1 presense from each ocean,  add presense on costal land range 1
				new GrowthOption( 
					new PushPresenceFromOcean(),
					new DrawPowerCard(),
					new PlacePresence(1, Target.Costal, "coatal" )
				)
			};

			InnatePowers = new InnatePower[]{
				InnatePower.For<OceanBreaksTheShore>(),
				InnatePower.For<PoundShipsToSplinters>()
			};

		}

		public override string Text => Name;

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Place in Ocean
			Presence.PlaceOn(board[0]);

			// !!! user gets to choose costal - but I am going to default to 3
			// could give ocean a 1 time growth option of Place-On-Costal
			Presence.PlaceOn(board[3]);

			gameState.InvaderMoved.Handlers.Add(InvadersMoved);
		}

		async Task InvadersMoved(GameState gs, InvaderMovedArgs args ) {
			if(args.to.Terrain!=Terrain.Ocean) return;

			drownedCount += args.Invader.Healthy.Health;
			var grp = (InvaderGroup)gs.InvadersOn( args.to );
			await grp.Destroy( 1,args.Invader );

			int spiritCount = gs.Spirits.Length;
			while(spiritCount <= drownedCount) {
				++Energy;
				drownedCount -= spiritCount;
			}

		}

		int drownedCount = 0;
	}

	public class OceanPresence : MyPresence {

		public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) { }

		public override void PlaceOn( Space space ) {
			base.PlaceOn( space );
			space.Board[0].TerrainForPower = Terrain.Wetland;
		}

		public override void RemoveFrom( Space space ) {
			base.RemoveFrom( space );
			var board = space.Board;
			if(!Spaces.Any(s=>s.Board == board))
				board[0].TerrainForPower = Terrain.Ocean;
		}

	}

}
