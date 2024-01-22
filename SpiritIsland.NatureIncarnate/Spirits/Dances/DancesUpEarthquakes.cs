namespace SpiritIsland.NatureIncarnate;

public class DancesUpEarthquakes : Spirit {

	public const string Name = "Dances Up Earthquakes";

	#region Custome Track spaces

	static SpiritAction SetImpendingEnergy(int impendingEnergyPerRound) => new SpiritAction(
		"Set Impending Energy/Round to "+impendingEnergyPerRound, 
		self=>((DancesUpEarthquakes)self).ImpendingEnergyPerRound = impendingEnergyPerRound
    );

	static Track One {
		get {
			var t = Track.MkEnergy(1);
			t.Action = SetImpendingEnergy(1);
			t.Icon = new IconDescriptor { BackgroundImg = Img.Coin, Text = "1",
				Sub = new IconDescriptor { 
					BackgroundImg = Img.ImpendingCard,
					ContentImg = Img.Coin,
					Text = "1"
				}
			};
			return t;
		}
	}

	static Track TwoImpendingEnergy => new Track("0/2") {
		Action = SetImpendingEnergy(2),
		Icon = new IconDescriptor {
			BackgroundImg = Img.ImpendingCard,
			ContentImg = Img.Coin,
			Text = "2",
		}
	};

	static Track FourAny => Track.MkEnergy(4,Element.Any);

	static Track MoonAndFire => new Track("moon,fire",Element.Moon,Element.Fire){
		Icon = new IconDescriptor {
			ContentImg = Img.Token_Moon,
			ContentImg2 = Img.Token_Fire,
		}
	};

	static Track AdditionalImpending => new Track("+1 Impending" ) {
		Icon = new IconDescriptor {
			Super = new IconDescriptor {  Text = "+1" },
			Sub = new IconDescriptor{ BackgroundImg = Img.ImpendingCard },
		},
		Action = new BoostImpendingPlays()
	};

	static Track GatherDahan => new Track( "Gather 1 Dahan" ) {
		Icon = new IconDescriptor { BackgroundImg = Img.Land_Gather_Dahan },
		Action = new Gather1Token(1,Human.Dahan), // !!! ??? implementation is optional, should it be required???
	};

	static Track MovePresence => new Track( "Moveonepresence.png" ) {
		Action = new MovePresence(1),
		Icon = new IconDescriptor { BackgroundImg = Img.MovePresence }
	};

	class BoostImpendingPlays : SpiritAction {
		public BoostImpendingPlays():base( "Boost Impending Plays" ) { }
		public override Task ActAsync( Spirit self ) {
			if(self is DancesUpEarthquakes due)
				++due.BonusImpendingPlays;
			return Task.CompletedTask;
		}

	}

	#endregion Custome Track spaces


	public DancesUpEarthquakes() : base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( One, MovePresence, Track.Energy2, AdditionalImpending, Track.Energy3, TwoImpendingEnergy, FourAny ),
			new PresenceTrack( Track.Card2, GatherDahan, MoonAndFire, AdditionalImpending, Track.EarthEnergy, Track.Card3, Track.Card4 )
		)
		, new GrowthTrack(
			new GrowthOption( new ReclaimAll(), new AddPresenceOrGainMajor() ),
			new GrowthOption( new GainPowerCard(), new PlacePresence( 1 ) ),
			new GrowthOption( new PlacePresence( 3 ), new AccelerateOrDelay(), new ReclaimN( 1 ) )
		)
		// Round 1
		, PowerCard.For(typeof(ResoundingFootfallsSowDismay)) // fast,3 - Impend
		, PowerCard.For(typeof(GiftOfSeismicEnergy))          // fast,3 - Impend
		// Round 2
		, PowerCard.For(typeof(ExaltationOfEchoedSteps))      // slow,? - Play 
		, PowerCard.For(typeof(RadiatingTremors))             // slow,2 - Impend
		// Round 3
		, PowerCard.For(typeof(RumblingsPortendAGreaterQuake))// fast,? - Play
			// Impend - new major
			// Impend - a minor
        // Round 4
		, PowerCard.For(typeof(InspireAWindingDance))		// slow     Play
			// Play - new major
			// Impend - remaining card
	) {

		// Innates
		InnatePowers = new InnatePower[] {
			InnatePower.For(typeof(LandCreaksWithTension)),
			InnatePower.For(typeof(EarthShuddersBuildingsFall))
		};

		Impending = [];
		decks.Add( new SpiritDeck { Type = SpiritDeck.DeckType.DaysThatNeverWere_Minor, Cards = Impending } );
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {

		// 1 in the highest-numbered mountain
		board.Spaces.Tokens()
			.Last( tokens => tokens.Dahan.Any )
			.Init( Presence.Token, 1 );

	}

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[]{
		new SpecialRule("Begin a Dance of Decades", "Whenever you would play a Power Card, you may instead pay any amountof Energy onto the card to make it an impending card."),
		new SpecialRule("Rhythmic Power Builds to a Cataclysmic Crescendo", "When you gain Energy from your Presence Track, also gain Impending-Energy onto each card made Impending on the previous turn. If any have enough energy, play it, discarding its energy. This costs no card plays.")
	};

	public override Task<PowerCard> ForgetACard( IEnumerable<PowerCard>? restrictedOptions = null, Present present = Present.Always ) 
		=> base.ForgetACard( restrictedOptions, present );

	#region Impending Cards

	public int ImpendingEnergyPerRound;
	public int BonusImpendingPlays;

	public List<PowerCard> Impending = [];
	public CountDictionary<string> ImpendingEnergy = [];

	protected override async Task ApplyRevealedPresenceTracks_Inner( Spirit self ) {
		ImpendingEnergyPerRound = 0;
		BonusImpendingPlays = 0;

		await base.ApplyRevealedPresenceTracks_Inner( self );

		// Add energy
		foreach(PowerCard card in Impending)  // don't use CountDictionary keys because they will be empty for count=0
			ImpendingEnergy[card.Name] += ImpendingEnergyPerRound;

		// remove any mature cards
		var mature = Impending.Where( c => c.Cost <= ImpendingEnergy[c.Name] ).ToArray();
		foreach(PowerCard card in mature)
			MoveMatureCard( card );
	}

	protected override async Task SelectAndPlayCardsFromHand_Inner( int remainingToPlay ) {
		int startingCount = InPlay.Count;
		await base.SelectAndPlayCardsFromHand_Inner( remainingToPlay );
		remainingToPlay -= (InPlay.Count - startingCount);
		await SelectImpendingCards( remainingToPlay );
	}

	void MoveMatureCard( PowerCard card ) {
		ImpendingEnergy[card.Name] = 0;
		Impending.Remove( card );
		InPlay.Add( card );
		AddElements( card );
		AddActionFactory( card );
	}

	async Task SelectImpendingCards( int remainingToPlay ) {
		remainingToPlay += BonusImpendingPlays;
		while(0 < remainingToPlay
			&& 0 < Hand.Count
			&& await SelectAndMakeImpending1( Hand, remainingToPlay )
		)
			--remainingToPlay;
	}

	async Task<bool> SelectAndMakeImpending1( IEnumerable<PowerCard> powerCardOptions, int remainingToPlay ) {
		string prompt = $"Make Impending power card ({remainingToPlay})";
		var card = await this.SelectPowerCard( prompt, powerCardOptions, CardUse.Impend, Present.Done );
		if(card == null) return false;

		if(!Hand.Contains( card )) 
			throw new CardNotAvailableException();

		Hand.Remove( card );
		Impending.Add( card );

		// add energy to card
		int energyToAdd = await this.SelectNumber( $"Add pending energy to {card.Name}.", Math.Min(card.Cost-1, Energy), 0 );
		Energy -= energyToAdd;
		ImpendingEnergy[card.Name] += energyToAdd;

		return true;
	}

	protected override IEnumerable<PowerCard> GetForgetableCards()
		=> base.GetForgetableCards().Union( Impending ).ToArray();

	#endregion

	#region Memento

	protected override object CustomMementoValue {
		get => new SavedCustomProps( this );
		set => ((SavedCustomProps)value).Restore( this );
	}

	class SavedCustomProps( DancesUpEarthquakes spirit ) {
		public void Restore( DancesUpEarthquakes spirit ) {
			spirit.Impending.Clear(); spirit.Impending.AddRange(_impending);
			spirit.ImpendingEnergy.Clear(); foreach(var p in _impendingEnergy) spirit.ImpendingEnergy.Add(p.Key,p.Value);
			spirit.ImpendingEnergyPerRound = _impendingEnergyPerRound;
			spirit.BonusImpendingPlays = _bonusImpendingPlays;

		}
		readonly PowerCard[] _impending = [.. spirit.Impending];
		readonly CountDictionary<string> _impendingEnergy = spirit.ImpendingEnergy.Clone();
		readonly int _impendingEnergyPerRound = spirit.ImpendingEnergyPerRound;
		readonly int _bonusImpendingPlays = spirit.BonusImpendingPlays;

	}

	#endregion Memento
}
