using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

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

	public class Thunderspeaker : Spirit {

		public const string Name = "Thunderspeaker";

		public override string Text => Name;

		public Thunderspeaker():base(
			new MyPresence(
				new Track[] { Track.Energy1, Track.AirEnergy, Track.Energy2, Track.FireEnergy, Track.SunEnergy, Track.Energy3 },
				new Track[] { Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Reclaim1, Track.Card3, Track.Card4 }
			),
			PowerCard.For<ManifestationOfPowerAndGlory>(),
			PowerCard.For<SuddenAmbush>(),
			PowerCard.For<VoiceOfThunder>(),
			PowerCard.For<WordsOfWarning>()
		) {
			GrowthOptions = new GrowthOption[]{
				new GrowthOption( 
					new ReclaimAll(), 
					new DrawPowerCard(1),
					new DrawPowerCard(1)
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
				new FastIf4Air<GatherTheWarriors>(),
				new FastIf4Air<LeadTheFuriousAssult>(),
			};

		}

		protected override void InitializeInternal( Board board, GameState gs ) {

			// Put 2 Presence on your starting board: 1 in each of the 2 lands with the most Dahanicon.png
			var spots = board.Spaces.OrderByDescending( gs.Dahan.Count ).Take( 2 ).ToArray();
			Presence.PlaceOn( spots[0] );
			Presence.PlaceOn( spots[1] );

			// Special Rules -Ally of the Dahan - Your presense may move with dahan
			gs.Dahan.Moved.Handlers.Add( MovePresenceWithDahan );

			// Special Rules - Sworn to Victory - For each dahan stroyed by invaders ravaging a land, destroy 1 of your presense withing 1
			gs.Dahan.Destroyed.Handlers.Add( DestroyNearbyPresence );
		}

		async Task MovePresenceWithDahan(GameState gs, DahanMovedArgs args) {
			int maxThatCanMove = Math.Min(args.count,Presence.CountOn(args.from));
			// 0 -> no action
			if(maxThatCanMove==0) return;
			var moveLookup = new Dictionary<string,int>();
			for(int i = maxThatCanMove; 0 < i; --i)
				moveLookup.Add($"Move {i} presence.",i );
			moveLookup.Add( "stay",0);
			
			string s = await this.SelectText("Move presence with dahan?", moveLookup.OrderByDescending(p=>p.Value).Select(p=>p.Key).ToArray()); 
			int countToMove = moveLookup[s];

			while(countToMove-- > 0)
				Presence.Move(args.from,args.to);

		}

		async Task DestroyNearbyPresence( GameState _, DahanDestroyedArgs args ) {
			if(args.Source != Cause.Invaders) return;

			string prompt = $"Sword to Victory: {args.count} dahan destroyed. Select presence to remove.";

			int numToDestroy = args.count;
			Space[] options;
			Space[] Calc() => args.space.Range( 1 ).Intersect( Presence.Spaces ).ToArray();

			while(numToDestroy-->0 && (options=Calc()).Length > 0)
				Presence.Destroy( await this.Action.Choose( new TargetSpaceDecision( prompt, options ) ));

		}

	}

}
