namespace SpiritIsland.NatureIncarnate;

public class WoundedWatersBleeding : Spirit, IHaveSecondaryElements {

	public const string Name = "Wounded Waters Bleeding";

	public override string Text => Name;

	#region Special Rules
	public override SpecialRule[] SpecialRules => _specialRules.ToArray();
	readonly List<SpecialRule> _specialRules = [ SeekingPath_Rule ];

	static readonly SpecialRule SeekingPath_Rule = new SpecialRule( "Seeking a Path Towards Healing", "After playing cards: (a) Claim a Healing Marker matching whichever water/animal you have more of. (b) Claim a Healing card if you meet requirments, (c) destroy 1 presence or forget a power card." );
	public void AddSpecialRule(SpecialRule rule ) {
		_specialRules.Add(rule);
	}
	#endregion

	#region constructor / initilization

	public WoundedWatersBleeding() : base(
		spirit => new WoundedPresence(spirit)
		,new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard(), new GainEnergy( 1 ) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 2 ) )
		)
		, PowerCard.For(typeof(BoonOfCorruptedBlood))    // fast
		, PowerCard.For(typeof(DrawToTheWatersEdge))     // fast
		, PowerCard.For(typeof(BloodWaterAndBloodlust))  // slow
		, PowerCard.For(typeof(WrackWithPainAndGrief))   // slow
	) {

		InnatePowers = new[] {
			InnatePower.For(typeof(SwirlAndSpill)),
			InnatePower.For(typeof(SanguinaryTaint))
		};

	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// put 2 presence in a land with blight.
		SpaceState blight = board.Spaces.ScopeTokens().First(t=>t.Blight.Any);
		blight.Init(Presence.Token,2);
		// put 2 presence and 1 blight in the highest numbered land with a town setup symbol
		SpaceState town = board.Spaces.ScopeTokens().Last( t => t.HasAny(Human.Town) ); // not exactly correct. Should looks at sumbols, not actual towns
		town.Init( Presence.Token, 2 );
		town.Setup(Token.Blight,1);

		// start with 4 energy
		Energy = 4;
	}

	#endregion

	#region Seeking a Path Towards Healing

	protected override async Task SelectAndPlayCardsFromHand_Inner( int numberToPlay ) {
		await base.SelectAndPlayCardsFromHand_Inner( numberToPlay );
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

		// !!! Instead of Asking separately, ask TOGETHER and let them pick (or not pick), Then just use .Get() without asking.
		int water = Elements.Get(Element.Water);
		int animal = Elements.Get(Element.Animal);
		if(animal == water - 1) // if animal is 1 behind
			animal = await Elements.GetAsync(Element.Animal); // ask if they want to make it a tie
		else if( water == animal - 1 )
			water = await Elements.GetAsync(Element.Water);

		var elementToSelect = water < animal ? Element.Animal
			: animal < water ? Element.Water
			: await this.SelectElementEx("Claim Healing Marker", new Element[] { Element.Water, Element.Animal} );
		HealingMarkers[elementToSelect]++;
	}

	async Task ClaimAHealingCard() {
		var options = HealingCards.Where(card=>card.MeetsRequirement(this) && !card.IsClaimed(this)).ToArray();
		if(options.Length == 0) return;
		var card = await SelectAsync(new A.TypedDecision<IHealingCard>("Claim Healing Card?", options, Present.Done));
		if(card == null) return;

		card.Claim(this);
		if(GrowthTrack.Groups.Length == 2) {
			var thirdGrowth = new GrowthGroup(
				new PlacePresence(3),
				new GainEnergy(3),
				new AddDestroyedPresence( 1 )
			);
			GrowthTrack = new( [.. GrowthTrack.Groups, thirdGrowth] );
			ActionScope.Current.Log(new Log.LayoutChanged($"Third growth added to {Name}"));
		}

	}

	async Task DestroyPresenceOrForgetCard() {
		await Cmd.Pick1(
			Cmd.DestroyPresence(),
			Cmd.ForgetPowerCard
		).ActAsync(this);
	}

	readonly IHealingCard[] HealingCards = [
		new RoilingWaters(),
		new SereneWaters(),
		new WatersRenew(),
		new WatersTasteOfRuin()
	];

	CountDictionary<Element> IHaveSecondaryElements.SecondaryElements => HealingMarkers;
	public CountDictionary<Element> HealingMarkers = [];
	public bool HealingCardClaimed => HealingCards.Any(c=>c.IsClaimed(this));

	#endregion Seeking a Path Towards Healing

	#region Memento

	protected override object CustomMementoValue {
		get => new SavedCustomProps( this );
		set => ((SavedCustomProps)value).Restore( this );
	}

	class SavedCustomProps( WoundedWatersBleeding spirit ) {
		public void Restore( WoundedWatersBleeding spirit ) {
			spirit.HealingMarkers[Element.Water] = _healingWatersMarkers;
			spirit.HealingMarkers[Element.Animal] = _healingAnimalMarkers;
			spirit.InnatePowers = _innates;
			spirit._specialRules.Clear(); spirit._specialRules.AddRange( _rules );
			spirit._seekHealing = _seekHealing;
		}
		readonly int _healingWatersMarkers = spirit.HealingMarkers[Element.Water];
		readonly int _healingAnimalMarkers = spirit.HealingMarkers[Element.Animal];
		readonly InnatePower[] _innates = (InnatePower[])spirit.InnatePowers.Clone();
		readonly SpecialRule[] _rules = [.. spirit._specialRules];
		readonly bool _seekHealing = spirit._seekHealing;
	}

	#endregion Memento

}