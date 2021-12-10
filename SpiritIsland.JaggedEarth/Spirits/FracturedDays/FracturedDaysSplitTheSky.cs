using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	// Add card to Days That Never Were

	// Power Cards

	// Innates


	public class FracturedDaysSplitTheSky : Spirit {

		public const string Name = "Fractured Days Split the Sky";
		public override string Text => Name;

		static readonly SpecialRule FragmentsOfScatteredTime = new SpecialRule(
			"Fragments of Scattered Time",
			"Many of your Powers require Time as an additional cost.  Spend it when you Resolve the Power. (Not when you play it.)  When you Gain1 Time, put 1 of your presence here from your presence track (or optinally, the island).  When you Spend 1 Time, return it to a presence track - or if you have to free spaces, Destory it."
		);

		static readonly SpecialRule DaysThatNeverWere = new SpecialRule(
			"Days that Never Were",
			"Your 3rd Growth option lets you gain any one Power Card from a special set you create during Setup.  When you gain a Power Card any other way, you may add one unchosen card to this set."
		);

		public override SpecialRule[] SpecialRules => new SpecialRule[] { FragmentsOfScatteredTime, DaysThatNeverWere };

		public FracturedDaysSplitTheSky():base(
			new SpiritPresence( 
				new PresenceTrack(Track.Energy1, Track.Energy1 , Track.Energy2, Track.Energy2, Track.Energy2, Track.Energy2 ),
				new PresenceTrack( Track.Card1, Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3 )
			)
			,PowerCard.For<AbsoluteStatis>()
			,PowerCard.For<BlurTheArcOfYears>()
			,PowerCard.For<PourTimeSideways>()
			,PowerCard.For<ThePastReturnsAgain>()
		) {
			var g2Repeater = new ActionRepeater(2);
			var g3Repeater = new ActionRepeater(3);

			Growth = new GrowthOptionGroup(
				new GrowthOption(
					new Gain1Element(Element.Air),
					new ReclaimAll(),
					new GainTime(2)
				),
				new GrowthOption(
					new Gain1Element(Element.Moon), 
					new DrawPowerCard(), 
					new PlacePresence(2), 
					g2Repeater.Bind( new GainTime(1) ),
					g2Repeater.Bind( new PlayExtraCardThisTurn(2) )
				),
				new GrowthOption(
					new Gain1Element(Element.Sun),
					new DrawPowerCardFromDaysThatNeverWere(), 
					g3Repeater.Bind( new GainTime(1) ),
					g3Repeater.Bind( new GainEnergy(2) )
				)
			);
			
			InnatePowers = new InnatePower[] {
				InnatePower.For<SlipTheFlowOfTime>(),
				InnatePower.For<VisionsOfAShiftingFuture>()
			};
		}

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// 1 in lowest numbered land with 1 dahan
			var space1 = board.Spaces.First(s=>gameState.Tokens[s].Dahan.Count==1);
			Presence.PlaceOn(space1, gameState);
			// 2 in highst numbered land without dahan
			var space2 = board.Spaces.Last( s => gameState.Tokens[s].Dahan.Count == 0 );
			Presence.PlaceOn( space2, gameState );
			Presence.PlaceOn( space2, gameState );

			
			// up as your initial Days That Never Were cards;
			int spiritCount = gameState.Spirits.Length;
			int cardsToDraw = 2 < spiritCount 
				? 4  // Deal 4 Minor and Major Powers face -
				: 6; // in a 1 or 2 - player game, instead deal 6 of each.
			DtnwMinor = gameState.MinorCards.Flip( cardsToDraw );
			DtnwMajor = gameState.MajorCards.Flip( cardsToDraw );

			// !!! In a 1 - board game, gain 1 Time.

		}

		public List<PowerCard> DtnwMinor { get; private set; }
		public List<PowerCard> DtnwMajor { get; private set; }

		// !!! instead create a Decks list and Fractured Days just adds 2 more to it.
		// get rid of the virtualness...
		public override SpiritDeck[] Decks => new SpiritDeck[]{
			new SpiritDeck{ Icon = Img.Deck_Hand, PowerCards = Hand },
			new SpiritDeck{ Icon = Img.Deck_Played, PowerCards = InPlay },
			new SpiritDeck{ Icon = Img.Deck_Discarded, PowerCards = DiscardPile },
			new SpiritDeck{ Icon = Img.Deck_DaysThatNeverWere_Minor, PowerCards = DtnwMinor },
			new SpiritDeck{ Icon = Img.Deck_DaysThatNeverWere_Major, PowerCards = DtnwMajor },
		};

		public async Task GainTime( int count, GameState gameState ) {
			while(count > 0) {

				string selectPrompt = $"Select presence to convert to Time ({count} remaining).";
				var from = (IOption)await Action.Decision( new Decision.Presence.SourceFromTrack( selectPrompt, this ) )
						?? (IOption)await Action.Decision( new Decision.Presence.Deployed( selectPrompt, this, Present.Done ) ); // Cancel

				await Presence.TakeFrom( from, gameState );
				Time++;

				--count;
			}
		}

		public int Time { get; set; }


		public override async Task<DrawCardResult> DrawMinor( GameState gameState, int numberToDraw = 4, int numberToKeep = 1 ) {
			var result = await base.DrawMinor( gameState, numberToDraw, numberToKeep );
			await Keep1ForDaysThatNeverWere(result);
			return result;
		}

		public override async Task<DrawCardResult> DrawMajor( GameState gameState, bool forgetCard = true, int numberToDraw = 4, int numberToKeep = 1 ) {
			var result = await base.DrawMajor( gameState, forgetCard, numberToDraw, numberToKeep );
			await Keep1ForDaysThatNeverWere( result );
			return result;
		}

		async Task Keep1ForDaysThatNeverWere( DrawCardResult drawResult ) {
			// select card to keep
			var keep = await this.SelectPowerCard("Keep 1 card for Days That Never Were", drawResult.Rejected, CardUse.AddToHand, Present.Always);
			// remove it from the rejected group
			drawResult.Rejected.Remove( keep );

			// Add to Days-That-Never-Were decks
			if( keep.PowerType == PowerType.Major)
				this.DtnwMajor.Add( keep );
			else
				this.DtnwMinor.Add( keep );
		}

	}

}
