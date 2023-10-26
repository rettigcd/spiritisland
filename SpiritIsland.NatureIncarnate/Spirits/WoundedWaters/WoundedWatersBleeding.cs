namespace SpiritIsland.NatureIncarnate;

public class WoundedWatersBleeding : Spirit, IHaveSecondaryElements {

	public const string Name = "Wounded Waters Bleeding";

	public override string Text => Name;

	#region Special Rules
	public override SpecialRule[] SpecialRules => _specialRules.ToArray();
	readonly List<SpecialRule> _specialRules = new List<SpecialRule> { SeekingPath_Rule };

	static readonly SpecialRule SeekingPath_Rule = new SpecialRule( "Seeking a Path Towards Healing", "After playing cards: (a) Claim a Healing Marker matching whichever water/animal you have more of. (b) Claim a Healing card if you meet requirments, (c) destroy 1 presence or forget a power card." );
	public void AddSpecialRule(SpecialRule rule ) {
		_specialRules.Add(rule);
	}
	#endregion

	#region constructor / initilization

	public WoundedWatersBleeding() : base(
		new WoundedPresence()
		, PowerCard.For<BoonOfCorruptedBlood>()    // fast
		, PowerCard.For<DrawToTheWatersEdge>()     // fast
		, PowerCard.For<BloodWaterAndBloodlust>()  // slow
		, PowerCard.For<WrackWithPainAndGrief>()   // slow
	) {
		GrowthTrack = new(
			new GrowthOption(
				new ReclaimAll(),
				new DrawPowerCard( 1 ),
				new GainEnergy( 1 )
			),
			new GrowthOption(
				new DrawPowerCard( 1 ),
				new PlacePresence( 2 )
			)
		);

		InnatePowers = new[] {
			InnatePower.For<SwirlAndSpill>(),
			InnatePower.For<SanguinaryTaint>()
		};

	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// put 2 presence in a land with blight.
		SpaceState blight = board.Spaces.Tokens().First(t=>t.Blight.Any);
		blight.Init(Presence.Token,2);
		// put 2 presence and 1 blight in the highest numbered land with a town setup symbol
		SpaceState town = board.Spaces.Tokens().Last( t => t.HasAny(Human.Town) ); // not exactly correct. Should looks at sumbols, not actual towns
		town.Init( Presence.Token, 2 );
		town.Adjust(Token.Blight,1);

		// start with 4 energy
		Energy = 4;
	}

	#endregion

	#region Seeking a Path Towards Healing

	public override async Task SelectAndPlayCardsFromHand( int? numberToPlay = null ) {
		await base.SelectAndPlayCardsFromHand( numberToPlay );
		await SeekAPathTowardsHealing();
	}

	public void StopHealing() { 
		_seekHealing = false;
		_specialRules.Remove( SeekingPath_Rule );
	}
	bool _seekHealing = true;

	async Task SeekAPathTowardsHealing() {
		if(!_seekHealing) return;
		await ClaimAHealingMarker();
		await ClaimAHealingCard();
		if(!_seekHealing) return;
		await DestroyPresenceOrForgetCard();
	}

	async Task ClaimAHealingMarker() {
		int water = Elements[Element.Water], animal = Elements[Element.Animal];
		var elementToSelect = water < animal ? Element.Animal
			: animal < water ? Element.Water
			: await this.SelectElementEx("Claim Healing Marker", new Element[] { Element.Water, Element.Animal} );
		HealingMarkers[elementToSelect]++;
	}

	async Task ClaimAHealingCard() {
		var options = HealingCards.Where(card=>card.MeetsRequirement(this) && !card.IsClaimed(this)).ToArray();
		if(options.Length == 0) return;
		var card = await Gateway.Decision(new Select.TypedDecision<IHealingCard>("Claim Healing Card?", options, Present.Done));
		if(card == null) return;

		card.Claim(this);
		if(GrowthTrack.Options.Length == 2) {
			var thirdGrowth = new GrowthOption(
				new PlacePresence( 3 ),
				new GainEnergy( 3 ),
				new PlaceDestroyedPresence( 1 )
			);
			GrowthTrack = new( GrowthTrack.Options.Append(thirdGrowth).ToArray() );
			GameState.Current.Log(new Log.LayoutChanged($"Third growth added to {Name}"));
		}

	}

	async Task DestroyPresenceOrForgetCard() {
		await Cmd.Pick1<SelfCtx>(
			Cmd.DestroyPresence(),
			Cmd.ForgetPowerCard
		).Execute(BindSelf());
	}

	readonly IHealingCard[] HealingCards = new IHealingCard[] {
		new RoilingWaters(),
		new SereneWaters(),
		new WatersRenew(),
		new WatersTasteOfRuin()
	};

	ElementCounts IHaveSecondaryElements.SecondaryElements => HealingMarkers;
	public ElementCounts HealingMarkers = new ElementCounts();
	public bool HealingCardClaimed => HealingCards.Any(c=>c.IsClaimed(this));

	#endregion Seeking a Path Towards Healing

	#region Memento

	class SavedCustomProps {
		public SavedCustomProps( WoundedWatersBleeding spirit ) {
			_healingWatersMarkers = spirit.HealingMarkers[Element.Water];
			_healingAnimalMarkers = spirit.HealingMarkers[Element.Animal];
			_innates = (InnatePower[])spirit.InnatePowers.Clone(); // don't use original because it gets updated
			_rules = spirit._specialRules.ToArray();
		}
		public void Restore( WoundedWatersBleeding spirit ) {
			spirit.HealingMarkers[Element.Water] = _healingWatersMarkers;
			spirit.HealingMarkers[Element.Animal] = _healingAnimalMarkers;
			spirit.InnatePowers = _innates;
			spirit._specialRules.Clear(); spirit._specialRules.AddRange( _rules );
		}
		readonly int _healingWatersMarkers;
		readonly int _healingAnimalMarkers;
		readonly InnatePower[] _innates;
		readonly SpecialRule[] _rules;
	}

	protected override object _customSaveValue {
		get => new SavedCustomProps(this);
		set => ((SavedCustomProps)value).Restore(this);
	}

	#endregion Memento

}