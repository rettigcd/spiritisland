﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	/*
	================================================================
	Bringer of Dreams and Nightmares
	* reclaim, +1 power card
	* reclaim 1, add presense range 0
	* +1 power card, +1 pressence range 1
	* add presense range Dahan or Invadors, +2 energy

	2 air 3 moon 4 any 5
	2 2 2 3 3 any

	Innate 1 - Spirits May Yet Dream => fast any spirit
	2 moon 2 air  Turn any face-down fear card face-up
	3 moon   Target spirit gains an element they have at least 1 of

	Innate 2 - Night Terrors => fast range 0, invaders
	1 moon 1 air    1 fear
	2 moon 1 air 1 beast   +1 fear
	3 moon 2 air 1 beast   +1 fear

	Special rules - To Dream a Thousand Deaths - Your power never cause Damange, nor can they destory anything other than your own presense 
	When a power of yours would destroy exporer/town/city generate 0/2/5 fear instead.  The power pushes explorers / towns it would destroy
	Setup:  2 presense in highest numbered sands

	Dread Apparations => 2 => fast, range 1, invaders => moon, air => When powers generate fear in target land, defend 1 per fear.  1 fear  (fear from to Dream a Thousands Deaths counts.  Fear from destroying town/cities does not.)
	Call on Midnight's Dream => 0 => fast, range 0, any => moon, animal => if target land has dahan, gain a major power.  If you Forget this Power, gain energy equal to dahan and you may play the major power immediately paying its cost -OR- if invaders are present, 2 fear
	Dreams of the Dahan => 0 => fast, range 2, any => moon, air => gather up to 2 dahan -OR- if target land has town/city, 1 fear for each dahan, to a maximum of 3 fear
	Predatory Nightmares => 2 => slow, 1 from sacred site, invaders => moon, fire, mountain, animal => 2 damange.  Push up to 2 dahan.  When your Powers would destroy invaders, instead they ggenerate fear and/or push those invaders

	*/

	public class Bringer : Spirit {

		public const string Name = "Bringer of Dreams and Nightmares";

		public override string Text => Name;

		public override string SpecialRules => "TO DREAM A THOUSAND DEATHS -  Your Powers never cause Damage, nor can they Destroy anything other than your own Presence. When your Powers would Destroy Invaders, instead generate 0/2/5 Fear and Pushes Invaders";

		public Bringer():base(
			new MyPresence(
				new PresenceTrack( Track.Energy2, Track.AirEnergy, Track.Energy3, Track.MoonEnergy, Track.Energy4, Track.AnyEnergy, Track.Energy5 ),
				new PresenceTrack( Track.Card2, Track.Card2, Track.Card2, Track.Card3, Track.Card3, Track.AnyEnergy )
			),
			PowerCard.For<CallOnMidnightsDream>(),
			PowerCard.For<DreadApparitions>(),
			PowerCard.For<DreamsOfTheDahan>(),
			PowerCard.For<PredatoryNightmares>()
		) {

			GrowthOptions = new GrowthOption[]{
				// reclaim, +1 power card
				new GrowthOption(new ReclaimAll(),new DrawPowerCard(1)),
				// reclaim 1, add presence range 0
				new GrowthOption(new Reclaim1(), new PlacePresence(0) ),
				// +1 power card, +1 pressence range 1
				new GrowthOption(new DrawPowerCard(1), new PlacePresence(1) ),
				// add presense range Dahan or Invadors, +2 energy
				new GrowthOption(new GainEnergy(2), new PlacePresence(4,Target.DahanOrInvaders))
			};

			this.InnatePowers = new InnatePower[]{
				InnatePower.For<SpiritsMayYetDream>(),
				InnatePower.For<NightTerrors>()
			};

		}


		protected override void InitializeInternal( Board board, GameState gs ) {
			// Setup: 2 presense in highest numbered sands
			var startingIn = board.Spaces.Where(x=>x.Terrain==Terrain.Sand).Last();
			Presence.PlaceOn( startingIn );
			Presence.PlaceOn( startingIn );
		}

		public override InvaderGroup BuildInvaderGroupForPowers( GameState gs, Space space ) {
			var src = gs.Tokens[space];

			var copy = new CountDictionary<Token>();
			foreach(var invader in src.Invaders())
				copy[invader] = src[invader];
			var detached = new TokenCountDictionary( space, copy );

			return new InvaderGroup( detached ) {
				DestroyInvaderStrategy = new ToDreamAThousandDeaths_DestroyStrategy( gs.Fear.AddDirect, Cause.Power, this.MakeDecisionsFor( gs ) ),
			};
		}

		public override Task DestroyDahanForPowers( GameState gs, Space space, int count, Token dahanToken ) {
			return Task.CompletedTask;
		}

	}

}
