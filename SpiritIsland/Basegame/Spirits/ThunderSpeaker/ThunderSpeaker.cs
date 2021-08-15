using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	/*
	=== Thunder Speaker

	* reclaim, +1 power card, +1 power card
	* +1 presense with 2 - contining dahan, +1 presense within 1 - containing dahan
	* +1 presense within 1, +4 energy

	1 air 2 fire sun 3
	1 2 2 3 reclaim-1 3 4

	Innate-1:  Gather the Warriors => slow, range 1, any
	4 air   this power may be fast
	1 beast    gather 1 dahan per air, push 1 dahan per sun

	Innate-2:  Lead the Furious Assult
	4 air   this power may be fast
	2 sun 1 fire   Destroy 1 town for every 2 dahan in target land
	4 sun 3 fire   Destory 1 city for every 3 dahan in target land

	Special Rules - Ally of the Dahan - Your presense may move with dahan
	Special Rules - Swort to Victory - For each dahan stroyed by invaters ravaging a land, destroy 1 of your presense withing 1
	Setup:  put 1 presense in each of the 2 lands with the most presense

	Sudden Ambush => 2 => fast, range 1, any => fire, air, animal => you may gather 1 dahan. Each dahan destroys 1 explorer
	Words of Warning => 1 => fast, range 1, has dahan => defend 3.  During ravage, dahan in target land deal damange simultaneously with invaders
	Manifestation of Power and Glory => 3 => slow, range 0, has dahan => sun, fire, air => 1 fear.  each dahan deals damange equal to the number of your presense in the target land
	Voice of thunder => 0 => slow, range 1, any => sun, air => push up to 4 dahan -OR- If invaders are present, 2 fear

	*/

	public class ThunderSpeaker : Spirit {

		public const string Name = "Thunder Speaker";

		public override string Text => Name;

		public ThunderSpeaker():base(
			new Track[] { Track.Energy1, Track.AirEnergy, Track.Energy2, Track.FireEnergy, Track.SunEnergy, Track.Energy3 },
			new Track[] { Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Reclaim1, Track.Card3, Track.Card4 },
			PowerCard.For<ManifestationOfPowerAndGlory>(),
			PowerCard.For<SuddenAmbush>(),
			PowerCard.For<VoiceOfThunder>(),
			PowerCard.For<WordsOfWarning>()
		) {
			GrowthOptions = new GrowthOption[]{
				new GrowthOption( 
					new ReclaimAll(), 
					new DrawPowerCard(2)
				),
				new GrowthOption( 
					new PlacePresence(2,Target.Dahan,"dahan"),
					new PlacePresence(1,Target.Dahan,"dahan")
				),
				new GrowthOption( 
					new PlacePresence(1), 
					new GainEnergy(4)
				)
			};

			this.InnatePowers = new InnatePower[]{
				InnatePower.For<GatherTheWarriors>(),
				InnatePower.For<GatherTheWarriors_Fast>(),
				InnatePower.For<LeadTheFuriousAssult>(),
				InnatePower.For<LeadTheFuriousAssult_Fast>(),
			};

			DrawPowerCard = new PowerProgression(
				PowerCard.For<VeilTheNightsHunt>(),
				PowerCard.For<ReachingGrasp>(),
				//PowerCard.For<WrapInWingsOfSunlight>(),      // Major
				PowerCard.For<Drought>(),
				PowerCard.For<ElementalBoon>(),

				// Borrowing Lightnings Swift Strike Power Progression until we get below implemented
				PowerCard.For<DelusionsOfDanger>(),
				PowerCard.For<CallToBloodshed>(),
				PowerCard.For<PowerStorm>(),
				PowerCard.For<PurifyingFlame>(),
				PowerCard.For<PillarOfLivingFlame>(),
				PowerCard.For<EntrancingApparitions>(),
				PowerCard.For<CallToIsolation>()

				//PowerCard.For<TalonsOfLightning>(),			 // Major
				//PowerCard.For<VigorOfTheBreakingDawn>(),	 // major
				//PowerCard.For<TheTreesAndStonesSpeakOfWar>(),// Major
			).DrawCard;


		}

		public override void Initialize( Board board, GameState gs ) {
			base.Initialize(board, gs);

			// Put 2 Presence on your starting board: 1 in each of the 2 lands with the most Dahanicon.png
			Presence.Place( board.Spaces.OrderByDescending(gs.GetDahanOnSpace).Take(2) );

			// Special Rules -Ally of the Dahan - Your presense may move with dahan
			gs.DahanMoved.Handlers.Add( MovePresenceWithDahan );

			// Special Rules - Sworn to Victory - For each dahan stroyed by invaders ravaging a land, destroy 1 of your presense withing 1
			gs.DahanDestroyed.Handlers.Add( DestroyNearbyPresence );
		}

		async Task MovePresenceWithDahan(GameState gs, DahanMovedArgs args) {
			int maxThatCanMove = Math.Min(args.count,Presence.On(args.from));
			// 0 -> no action
			if(maxThatCanMove==0) return;
			var moveLookup = new Dictionary<string,int>();
			for(int i = maxThatCanMove; 0 < i; --i)
				moveLookup.Add($"Move {i} presence.",i );
			moveLookup.Add( "stay",0);
			
			string s = await new ActionEngine( this,gs )
				.SelectText("Move presence with dahan?", moveLookup.OrderByDescending(p=>p.Value).Select(p=>p.Key).ToArray()); 
			int countToMove = moveLookup[s];

			while(countToMove-- > 0)
				Presence.Move(args.from,args.to);

		}

		async Task DestroyNearbyPresence( GameState gameState, DahanDestroyedArgs args ) {
			if(args.Source != DahanDestructionSource.Invaders) return;

			string prompt = $"Sword to Victory: {args.count} dahan destroyed. Select presence to remove.";
			var eng = new ActionEngine( this, gameState );

			int numToDestroy = args.count;
			Space[] options;
			Space[] Calc() => args.space.SpacesWithin( 1 ).Intersect( Presence.Spaces ).ToArray();

			while(numToDestroy-->0 && (options=Calc()).Length > 0)
				Presence.Destroy( await eng.SelectSpace( prompt, options ) );

		}

	}

}
