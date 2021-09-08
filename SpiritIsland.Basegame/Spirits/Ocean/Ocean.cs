
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
		public override string SpecialRules => "OCEAN IN PLAY - You may add/move Presence into Oceans, but may not add/move Presence into Inland lands. On boards where you have 1 or more Presenceicon.png, Oceans are treated as Coastal Wetlands for Spirit Powers/Special Rules and Blighticon.png.You Drown any Invaders or Dahanicon.png moved to those Oceans."
			+"\r\n" + "DROWNING - Destroy Drowned pieces, placing Drowned Invaders here. At any time you may exchange (X) Health of these Invaders for 1 Energy, where X = number of players.";

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

			this.AddActionFactory( new Setup_PlacePresenceInCostal() ); // let user pick initial ocean

			gameState.Tokens.TokenMoved.ForGame.Add(InvadersMoved);
		}

		async Task InvadersMoved(GameState gs, TokenMovedArgs args ) {
			if(args.to.Terrain!=Terrain.Ocean) return;
			var grp = args.Token.Generic;
			if( grp != Invader.City && grp != Invader.Town && grp != Invader.Explorer ) return; // Could created an Invader subclass that is easier to test.

			drownedCount += args.Token.FullHealth;
			await gs.Invaders.On( args.to, Cause.Ocean ).Destroy( 1,args.Token );

			int spiritCount = gs.Spirits.Length;
			while(spiritCount <= drownedCount) {
				++Energy;
				drownedCount -= spiritCount;
			}

		}

		int drownedCount = 0;
	}

	class Setup_PlacePresenceInCostal : GrowthActionFactory {
		// ! Can't used normal PlacePresence, because it must be range-1, range 0 not allowed.
		public override async Task ActivateAsync( Spirit spirit, GameState gameState ) {
			var options = spirit.Presence.Spaces.First().Adjacent;
			var space = await spirit.Action.Decide( new TargetSpaceDecision( "Add presence to", options ) );
			spirit.Presence.PlaceOn( space );
		}
	}


	public class OceanPresence : MyPresence {

		public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) { }

		public override void PlaceOn( Space space ) {
			base.PlaceOn( space );
			// Mark the ocean on this board as a Wetland
			space.Board[0].TerrainForPower = Terrain.Wetland;
		}

		public override void RemoveFrom( Space space ) {
			base.RemoveFrom( space );
			var board = space.Board;
			// If no ocean left on this board
			if(!Spaces.Any(s=>s.Board == board))
				// restore Ocean to an Ocean terrain
				board[0].TerrainForPower = Terrain.Ocean;
		}

	}

}
