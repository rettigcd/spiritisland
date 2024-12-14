namespace SpiritIsland.JaggedEarth;

// Boards
// S:	E,B,C
// A:	F,H,G
// B:	A,D


public class ShiftingMemoryOfAges : Spirit, IHaveSecondaryElements {

	public const string Name = "Shifting Memory Of Ages";
	public override string SpiritName => Name;

	static readonly SpecialRule LongAges = new SpecialRule(
		"Long Ages of Knowledge and Forgetfulness", 
		"When you would Forget a Power Card from your hand, you may instead discard it."
	);

	#region Presence Track Helpers

	static Track Prepare(int energy){
		return new Track( energy + " energy, prepare-El" ) {
			Energy = energy,
			Icon = new IconDescriptor { 
				BackgroundImg = Img.Coin, 
				Text = energy.ToString(), 
				Sub = new IconDescriptor { BackgroundImg = Img.ShiftingMemory_PrepareEl }
			},
			Action = new PrepareElement($"{energy} energy"),
		};
	}

	static Track DiscardElementsForCardPlay => new Track("discard 2 elements for card play" ) { 
		Action = new DiscardElementsForCardPlay(2),
		Icon = new IconDescriptor { BackgroundImg = Img.ShiftingMemory_Discard2Prep },
	};

	#endregion Presence Track Helpers

	#region constructor / initialization

	public ShiftingMemoryOfAges() 
		:base(
			spirit => new SpiritPresence( spirit,
				new PresenceTrack(Track.Energy0,Track.Energy1,Track.Energy2,Prepare(3),Track.Energy4,Track.Reclaim1Energy,Track.Energy5,Prepare(6)),
				new PresenceTrack(Track.Card1,Track.Card2,Track.Card2,DiscardElementsForCardPlay,Track.Card3)
			)
			,new GrowthTrack(
				new GrowthGroup( new ReclaimAll(), new PlacePresence( 0 ) ),
				new GrowthGroup( new GainPowerCard(), new PlacePresence( 2 ) ),
				new GrowthGroup( new PlacePresence( 1 ), new GainEnergy( 2 ) ),
				new GrowthGroup( new GainEnergy( 9 ) )
			)
			,PowerCard.For(typeof(BoonOfAncientMemories))
			,PowerCard.For(typeof(ElementalTeachings))
			,PowerCard.For(typeof(ShareSecretsOfSurvival))
			,PowerCard.For(typeof(StudyTheInvadersFears))
		) {

		InnatePowers = [
			new UserSelectedInnatePower(typeof(LearnTheInvadersTactics)), 
			new UserSelectedInnatePower(typeof(ObserveTheEverChangingWorld))
		];

		SpecialRules = [LongAges, InsightsIntoTheWorldsNature.Rule];
		Elements = new InsightsIntoTheWorldsNature(this);
	}

	protected override void InitializeInternal(Board board, GameState gameState) {
		// Put 2 presence on your starting board in the highest-number land that is Sands or Mountain.
		var space = board.Spaces.Last(x => x.IsOneOf(Terrain.Sands, Terrain.Mountain)).ScopeSpace;
		space.Setup(Presence.Token, 2);

		// Prepare 1 moon, 1 air, and 1 earth marker. (++ allows us to use SMOA for testing, where =1 overwrites testing values)
		PreparedElements[Element.Moon]++;
		PreparedElements[Element.Air]++;
		PreparedElements[Element.Earth]++;
	}

	#endregion constructor / initialization

	#region Forget Power changes

	public override async Task<PowerCard> ForgetACard( IEnumerable<PowerCard> restrictedOptions = null, Present present = Present.Always ) {
		IEnumerable<SingleCardUse> options = SingleCardUse.GenerateUses(CardUse.Discard,InPlay.Union( Hand ))
			.Union( SingleCardUse.GenerateUses(CardUse.Forget,DiscardPile) )
			.Where(u => restrictedOptions==null || restrictedOptions.Contains(u.Card));
				
		var decision = new A.PowerCard( "Select card to forget or discard", options, present );
		PowerCard cardToForgetOrDiscard = await SelectAsync( decision );
		if(cardToForgetOrDiscard != null)
			ForgetThisCard( cardToForgetOrDiscard );
		return cardToForgetOrDiscard != null && !DiscardPile.Contains(cardToForgetOrDiscard) 
			? cardToForgetOrDiscard	// card not in discard pile, must have been forgotten
			: null; 
	}

	/// <summary>
	/// If in hand, allows discarding instead of forgetting.
	/// </summary>
	public override void ForgetThisCard( PowerCard card ) {

		// (Source-1) Purchased / Active
		if(InPlay.Contains( card )) {
			foreach(var el in card.Elements) Elements.Remove(el.Key,el.Value);// lose elements from forgotten card
			InPlay.Remove( card );
			DiscardPile.Add( card );
			return;
		} 

		if(Hand.Remove( card )) {
			DiscardPile.Add( card );
			return;
		}

		if(DiscardPile.Contains( card )) {
			base.ForgetThisCard( card );
			return;
		}

		throw new System.Exception("Can't find card to forget:"+card.Title);
	}

	#endregion Forget Power changes

	#region Elements

	public async Task PrepareElement(string context) {
		// This is only used by Shifting Memories
		var el = await this.SelectElementEx($"Prepare Element ({context})", ElementList.AllElements);
		PreparedElements[el]++;
	}

	public CountDictionary<Element> PreparedElements => ((InsightsIntoTheWorldsNature)Elements).PreparedElements;

	public async Task<CountDictionary<Element>> DiscardElements(int totalNumToRemove, string effect ) {
		var discarded = new CountDictionary<Element>();

		int index = 0;
		while(index++ < totalNumToRemove) {
			Element el = await this.SelectElementEx($"Select element to discard for {effect} ({index} of {totalNumToRemove})",PreparedElements.Keys, Present.Done);
			if( el == default ) break;
			PreparedElements[el]--;
			discarded[el]++;
		}
		return discarded;
	}

	CountDictionary<Element> IHaveSecondaryElements.SecondaryElements => PreparedElements;

	#endregion Elements

	protected override object CustomMementoValue { 
		get => PreparedElements.ToArray();
		set => InitFromArray( PreparedElements, (KeyValuePair<Element, int>[])value );
	}

}
