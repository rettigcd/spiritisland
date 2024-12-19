namespace SpiritIsland.JaggedEarth;

public class FracturedDaysSplitTheSky : Spirit {

	public const string Name = "Fractured Days Split the Sky";
	public override string SpiritName => Name;

	static readonly SpecialRule FragmentsOfScatteredTime = new SpecialRule(
		"Fragments of Scattered Time",
		"Many of your Powers require Time as an additional cost.  Spend it when you Resolve the Power. (Not when you play it.)  When you Gain1 Time, put 1 of your presence here from your presence track (or optinally, the island).  When you Spend 1 Time, return it to a presence track - or if you have to free spaces, Destroy it."
	);

	static GrowthTrack BuildGrowthTrack() {
		var g2Repeater = new ActionRepeater( 2 );
		var g3Repeater = new ActionRepeater( 3 );

		return new GrowthTrack(
			new GrowthGroup(
				new GainAllElements( Element.Air ),
				new ReclaimAll(),
				new GainTime( 2 )
			),
			new GrowthGroup(
				new GainAllElements( Element.Moon ),
				new GainPowerCard(),
				new PlacePresence( 2 ),
				g2Repeater.BindSelfCmd( new GainTime( 1 ) ),
				g2Repeater.BindSelfCmd( new PlayExtraCardThisTurn( 2 ) )
			),
			new GrowthGroup(
				new GainAllElements( Element.Sun ),
				new DrawPowerCardFromDaysThatNeverWere(),
				new MovePresence( 4 ),
				g3Repeater.BindSelfCmd( new GainTime( 1 ) ),
				g3Repeater.BindSelfCmd( new GainEnergy( 2 ) )
			)
		);
	}

	public FracturedDaysSplitTheSky():base(
		x=> new SpiritPresence( x,
			new PresenceTrack(Track.Energy1, Track.Energy1 , Track.Energy2, Track.Energy2, Track.Energy2, Track.Energy2 ),
			new PresenceTrack( Track.Card1, Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3 )
		)
		,BuildGrowthTrack()
		,PowerCard.ForDecorated(AbsoluteStasis.ActAsync)
		,PowerCard.ForDecorated(BlurTheArcOfYears.ActAsync)
		,PowerCard.ForDecorated(PourTimeSideways.ActAsync)
		,PowerCard.ForDecorated(ThePastReturnsAgain.ActAsync)
	) {
		InnatePowers = [
			InnatePower.For(typeof(SlipTheFlowOfTime)),
			InnatePower.For(typeof(VisionsOfAShiftingFuture))
		];

		DtnwMinor = [];
		DtnwMajor = [];
		decks.Add( new SpiritDeck{ Type = SpiritDeck.DeckType.DaysThatNeverWere_Minor, Cards = DtnwMinor } );
		decks.Add( new SpiritDeck{ Type = SpiritDeck.DeckType.DaysThatNeverWere_Major, Cards = DtnwMajor } );
		Draw = new DaysThatNeverWere(this);
		SpecialRules = [FragmentsOfScatteredTime, DaysThatNeverWere.Rule];
	}

	OneOrTwoClass? _randomizer;
	public int OneOrTwo() => _randomizer!.Next(); // access repeatable random # for Visions of Shifting Future

	protected override void InitializeInternal( Board board, GameState gs ) {
		_randomizer = new OneOrTwoClass( gs.ShuffleNumber + 2837 ); // 2837 so not using same shuffle as others

		// 1 in lowest-numbered land with 1 dahan
		var lowestLandWith1Dahan = gs.Tokens[ board.Spaces.First(s=>s.ScopeSpace.Dahan.CountAll==1) ];
		lowestLandWith1Dahan.Setup(Presence.Token, 1);

		// 2 in highst numbered land without dahan
		var space2 = board.Spaces.ScopeTokens().Last( s => s.Dahan.CountAll == 0 );
		space2.Setup( Presence.Token, 2 );
			
		// up as your initial Days That Never Were cards;
		int spiritCount = gs.Spirits.Length;
		int cardsToDraw = 2 < spiritCount 
			? 4  // Deal 4 Minor and Major Powers face -
			: 6; // in a 1 or 2 - player game, instead deal 6 of each.
		DtnwMinor.AddRange( gs.MinorCards!.Flip( cardsToDraw ) );
		DtnwMajor.AddRange( gs.MajorCards!.Flip( cardsToDraw ) );

		// In a 1 - board game, gain 1 Time.
		if( gs.Spirits.Length == 1 )
			++Time;
	}

	public List<PowerCard> DtnwMinor { get; private set; }
	public List<PowerCard> DtnwMajor { get; private set; }

	public async Task GainTime( int count ) {
		while(0 < count) {

			var from = await this.SelectAlways(
				Prompts.SelectPresenceTo($"convert to Time ({count} remaining)."),
				this.DeployablePresence()
			);

			if( from == null) break; // !!! is this optional or not ???

			await from.RemoveAsync();
			Time++;

			--count;
		}
	}

	public async Task SpendTime( int count ) {
		var hide = await Select( A.TrackSlot.ToCover( this ) );

		Time -= count;
		if(hide != null)
			await Presence.Return(hide);
			
	}

	public int Time { get; private set; }

	//async Task Keep1ForDaysThatNeverWere( DrawCardResult drawResult ) {
	//	// select card to keep
	//	var keep = await this.SelectPowerCard("Keep 1 card for Days That Never Were", 1, drawResult.Rejected, CardUse.AddToHand, Present.Always);
	//	// remove it from the rejected group
	//	drawResult.Rejected.Remove( keep );

	//	// Add to Days-That-Never-Were decks
	//	if( keep.PowerType == PowerType.Major)
	//		this.DtnwMajor.Add( keep );
	//	else
	//		this.DtnwMinor.Add( keep );
	//}

	#region Custom Memento

	protected override object? CustomMementoValue { 
		get => new FracturedDaysMemento(this);
		set { if(value is FracturedDaysMemento x) x.Restore(this); }
	}

	class FracturedDaysMemento( FracturedDaysSplitTheSky _spirit ) {
		public void Restore( FracturedDaysSplitTheSky spirit ) {
			spirit._randomizer!.Memento = _random;
			spirit.Time = _time;
			spirit.DtnwMinor.SetItems(_minor);
			spirit.DtnwMajor.SetItems(_major);
		}
		readonly object _random = _spirit._randomizer!.Memento;
		readonly int _time = _spirit.Time;
		readonly PowerCard[] _minor = [.._spirit.DtnwMinor];
		readonly PowerCard[] _major = [.._spirit.DtnwMajor];
	}

	#endregion Custom Memento

	/// <summary> Randomizer with a state that can be restored. </summary>
	class OneOrTwoClass( int seed ) : IHaveMemento {
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
		readonly Random _randomizer = new Random( seed );
	}

	#region Repeater

	/// <summary>
	/// Manages a group Growth Actions that can be repeated N times:
	///  - Restoring the repeatable actions when repeats remain
	///  - Clearing out all repeatable actions when repeats are used up.
	/// </summary>
	/// <remarks>
	/// Fractured Days Growth Option 2 & 3
	/// </remarks>
	public class ActionRepeater(int repeats) {

		public readonly int _repeats = repeats;
		public int _remainingRepeats;

		#region constructor

		#endregion

		public void Register(IHelpGrowActionFactory factory) {
			_factories.Add(factory);
		}

		public void BeginAction() {
			if( _remainingRepeats == 0 )
				_remainingRepeats = _repeats;
		}

		public void EndAction(Spirit spirit) {
			--_remainingRepeats;

			if( 0 < _remainingRepeats )
				RestoreActionFactoryToAvailableActions(spirit);
			else
				RemoveUnusedActions(spirit);
		}

		/// <summary>
		/// Creates a Command that will count against the Repeater's max # of repeats.
		/// </summary>
		public SpiritAction BindSelfCmd(SpiritAction inner) => new RepeatableSelfCmd(inner, this);

		#region private

		void RestoreActionFactoryToAvailableActions(Spirit spirit) {
			var remaining = spirit.GetAvailableActions(Phase.Growth).ToArray();
			foreach( var factory in _factories )
				if( !remaining.Contains(factory) )
					spirit.AddActionFactory(factory);
		}

		/// <summary>
		/// Remove the 'other' non-used Actions that are part of this Repeater group.
		/// </summary>
		/// <param name="spirit"></param>
		void RemoveUnusedActions(Spirit spirit) {
			var remaining = spirit.GetAvailableActions(Phase.Growth).ToArray();
			foreach( var factory in _factories )
				if( remaining.Contains(factory) )
					spirit.RemoveFromUnresolvedActions(factory);
		}

		readonly List<IHelpGrowActionFactory> _factories = [];

		#endregion

	}

	/// <summary>
	/// Created by ActionRepeater
	/// Wraps another cmd and lets it be treated as a group and repeated.
	/// </summary>
	public class RepeatableSelfCmd : SpiritAction {

		public SpiritAction Inner { get; }

		internal RepeatableSelfCmd(SpiritAction inner, ActionRepeater repeater)
			: base(inner.Description + "x" + repeater._repeats) {
			Inner = inner;
			_repeater = repeater;
			repeater.Register(new GrowthAction(this));
		}

		public override async Task ActAsync(Spirit self) {
			_repeater.BeginAction();
			await Inner.ActAsync(self);
			_repeater.EndAction(self);
		}

		readonly ActionRepeater _repeater;

	}

	#endregion
}

class DaysThatNeverWere(Spirit s) : DrawCardStrategy(s) {

	public const string Name = "Days that Never Were";
	const string Description = "Your 3rd Growth option lets you gain any one Power Card from a special set you create during Setup.  When you gain a Power Card any other way, you may add one unchosen card to this set.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	protected override async Task<DrawCardResult> Inner(PowerCardDeck deck, int numberToDraw, int numberToKeep, bool forgetACard) {
		var result = await base.Inner(deck, numberToDraw, numberToKeep, forgetACard);
		await Keep1ForDaysThatNeverWere(result);
		return result;
	}

	// !! This is really just an event handler; - Could implement as an event instead of diriving a new class
	async Task Keep1ForDaysThatNeverWere(DrawCardResult drawResult) {
		FracturedDaysSplitTheSky fdstk = (FracturedDaysSplitTheSky)_spirit;

		// select card to keep
		var keep = (await _spirit.SelectPowerCard("Keep 1 card for Days That Never Were", 1, drawResult.Rejected!, CardUse.AddToHand, Present.Always))!;
		// remove it from the rejected group
		drawResult.Rejected!.Remove(keep);

		// Add to Days-That-Never-Were decks
		if( keep.PowerType == PowerType.Major )
			fdstk.DtnwMajor.Add(keep);
		else
			fdstk.DtnwMinor.Add(keep);
	}
}
