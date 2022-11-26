namespace SpiritIsland.JaggedEarth;

public class FracturedDaysSplitTheSky : Spirit {

	public const string Name = "Fractured Days Split the Sky";
	public override string Text => Name;

	static readonly SpecialRule FragmentsOfScatteredTime = new SpecialRule(
		"Fragments of Scattered Time",
		"Many of your Powers require Time as an additional cost.  Spend it when you Resolve the Power. (Not when you play it.)  When you Gain1 Time, put 1 of your presence here from your presence track (or optinally, the island).  When you Spend 1 Time, return it to a presence track - or if you have to free spaces, Destroy it."
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
		,PowerCard.For<AbsoluteStasis>()
		,PowerCard.For<BlurTheArcOfYears>()
		,PowerCard.For<PourTimeSideways>()
		,PowerCard.For<ThePastReturnsAgain>()
	) {
		var g2Repeater = new ActionRepeater(2);
		var g3Repeater = new ActionRepeater(3);

		GrowthTrack = new GrowthTrack(
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
				new MovePresence(4),
				g3Repeater.Bind( new GainTime(1) ),
				g3Repeater.Bind( new GainEnergy(2) )
			)
		);
			
		InnatePowers = new InnatePower[] {
			InnatePower.For<SlipTheFlowOfTime>(),
			InnatePower.For<VisionsOfAShiftingFuture>()
		};

		DtnwMinor = new List<PowerCard>();
		DtnwMajor = new List<PowerCard>();
		decks.Add( new SpiritDeck{ Icon = Img.Deck_DaysThatNeverWere_Minor, PowerCards = DtnwMinor } );
		decks.Add( new SpiritDeck{ Icon = Img.Deck_DaysThatNeverWere_Major, PowerCards = DtnwMajor } );

	}

	protected override void InitializeInternal( Board board, GameState gs ) {

		// 1 in lowest numbered land with 1 dahan
		var space1 = gs.Tokens[ board.Spaces.First(s=>gs.Tokens[s].Dahan.Count==1) ];
		Presence.Adjust( space1, 1 );

		// 2 in highst numbered land without dahan
		var space2 = gs.Tokens[ board.Spaces.Last( s => gs.Tokens[s].Dahan.Count == 0 ) ];
		Presence.Adjust( space2, 2 );
			
		// up as your initial Days That Never Were cards;
		int spiritCount = gs.Spirits.Length;
		int cardsToDraw = 2 < spiritCount 
			? 4  // Deal 4 Minor and Major Powers face -
			: 6; // in a 1 or 2 - player game, instead deal 6 of each.
		DtnwMinor.AddRange( gs.MinorCards.Flip( cardsToDraw ) );
		DtnwMajor.AddRange( gs.MajorCards.Flip( cardsToDraw ) );

		// In a 1 - board game, gain 1 Time.
		if( gs.Spirits.Length == 1 )
			++Time;
	}

	public List<PowerCard> DtnwMinor { get; private set; }
	public List<PowerCard> DtnwMajor { get; private set; }

	public async Task GainTime( int count, GameState gameState ) {
		while(count > 0) {

			string selectPrompt = $"Select presence to convert to Time ({count} remaining).";
			var from = (IOption)await Action.Decision( Select.TrackSlot.ToReveal( selectPrompt, this, gameState ) )
					?? (IOption)await Action.Decision( Select.DeployedPresence.All( selectPrompt, new ReadOnlyBoundPresence( this, gameState, gameState.Island.Terrain), Present.Done ) ); // Cancel

			await Presence.TakeFrom( from, gameState );
			Time++;

			--count;
		}
	}

	public async Task SpendTime( int count ) {
		var hide = await Action.Decision( Select.TrackSlot.ToCover( this ) );

		Time -= count;
		if(hide != null)
			await Presence.Return(hide);
			
	}

	public int Time { get; private set; }


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