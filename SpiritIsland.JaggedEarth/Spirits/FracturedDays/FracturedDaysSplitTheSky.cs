using System.Runtime.Serialization.Formatters.Binary;

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

	static GrowthTrack BuildGrowthTrack() {
		var g2Repeater = new ActionRepeater( 2 );
		var g3Repeater = new ActionRepeater( 3 );

		return new GrowthTrack(
			new GrowthOption(
				new GainAllElements( Element.Air ),
				new ReclaimAll(),
				new GainTime( 2 )
			),
			new GrowthOption(
				new GainAllElements( Element.Moon ),
				new GainPowerCard(),
				new PlacePresence( 2 ),
				g2Repeater.BindSelfCmd( new GainTime( 1 ) ),
				g2Repeater.BindSelfCmd( new PlayExtraCardThisTurn( 2 ) )
			),
			new GrowthOption(
				new GainAllElements( Element.Sun ),
				new DrawPowerCardFromDaysThatNeverWere(),
				new MovePresence( 4 ),
				g3Repeater.BindSelfCmd( new GainTime( 1 ) ),
				g3Repeater.BindSelfCmd( new GainEnergy( 2 ) )
			)
		);
	}

	public override SpecialRule[] SpecialRules => new SpecialRule[] { FragmentsOfScatteredTime, DaysThatNeverWere };

	public FracturedDaysSplitTheSky():base(
		x=> new SpiritPresence( x,
			new PresenceTrack(Track.Energy1, Track.Energy1 , Track.Energy2, Track.Energy2, Track.Energy2, Track.Energy2 ),
			new PresenceTrack( Track.Card1, Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3 )
		)
		, BuildGrowthTrack()
		, PowerCard.For(typeof(AbsoluteStasis))
		,PowerCard.For(typeof(BlurTheArcOfYears))
		,PowerCard.For(typeof(PourTimeSideways))
		,PowerCard.For(typeof(ThePastReturnsAgain))
	) {
		InnatePowers = [
			InnatePower.For(typeof(SlipTheFlowOfTime)),
			InnatePower.For(typeof(VisionsOfAShiftingFuture))
		];

		DtnwMinor = [];
		DtnwMajor = [];
		decks.Add( new SpiritDeck{ Type = SpiritDeck.DeckType.DaysThatNeverWere_Minor, Cards = DtnwMinor } );
		decks.Add( new SpiritDeck{ Type = SpiritDeck.DeckType.DaysThatNeverWere_Major, Cards = DtnwMajor } );

	}

	OneOrTwoClass _randomizer;
	public int OneOrTwo() => _randomizer.Next(); // access repeatable random # for Visions of Shifting Future

	protected override void InitializeInternal( Board board, GameState gs ) {
		_randomizer = new OneOrTwoClass( gs.ShuffleNumber + 2837 ); // 2837 so not using same shuffle as others

		// 1 in lowest-numbered land with 1 dahan
		var lowestLandWith1Dahan = gs.Tokens[ board.Spaces.First(s=>s.Tokens.Dahan.CountAll==1) ];
		lowestLandWith1Dahan.Setup(Presence.Token, 1);

		// 2 in highst numbered land without dahan
		var space2 = board.Spaces.Tokens().Last( s => s.Dahan.CountAll == 0 );
		space2.Setup( Presence.Token, 2 );
			
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

	public async Task GainTime( int count ) {
		while(0 < count) {

			string selectPrompt = $"convert to Time ({count} remaining).";

			var from = await this.SelectSourcePresence( Present.Done, selectPrompt ); // Come from track or board
			if(from == null) break; // !!! is this optional or not ???

			await from.RemoveAsync();
			Time++;

			--count;
		}
	}

	public async Task SpendTime( int count ) {
		var hide = await SelectAsync( A.TrackSlot.ToCover( this ) );

		Time -= count;
		if(hide != null)
			await Presence.Return(hide);
			
	}

	public int Time { get; private set; }

	protected override async Task<DrawCardResult> DrawInner( PowerCardDeck deck, int numberToDraw, int numberToKeep, bool forgetACard ){
		var result = await base.DrawInner( deck, numberToDraw, numberToKeep, forgetACard );
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

	#region Custom Memento

	protected override object CustomMementoValue { 
		get => new FracturedDaysMemento(this);
		set => ((FracturedDaysMemento)value).Restore(this);
	}

	class FracturedDaysMemento {
		public FracturedDaysMemento(FracturedDaysSplitTheSky spirit) {
			_random = spirit._randomizer.Memento;
			_time = spirit.Time;
			_minor = [.. spirit.DtnwMinor];
			_major = [.. spirit.DtnwMajor];
		}
		public void Restore( FracturedDaysSplitTheSky spirit ) {
			spirit._randomizer.Memento = _random;
			spirit.Time = _time;
			spirit.DtnwMinor.SetItems(_minor);
			spirit.DtnwMajor.SetItems(_major);
		}
		readonly object _random;
		readonly int _time;
		readonly PowerCard[] _minor;
		readonly PowerCard[] _major;

	}

	#endregion Custom Memento

	/// <summary> Randomizer with a state that can be restored. </summary>
	class OneOrTwoClass : IHaveMemento {
		public OneOrTwoClass( int seed ) {
			_randomizer = new Random( seed );
		}
		public int Next() {
			while(_history.Count <= _cur)
				_history.Add( _randomizer.Next( 2 ) );
			return _history[_cur++];
		}
		public object Memento {
			get => _cur;
			set => _cur = (int)value;
		}
		readonly List<int> _history = [];
		int _cur = 0;
		readonly Random _randomizer;
	}


}