namespace SpiritIsland.JaggedEarth;

public class ShiftingMemoryOfAges : Spirit, IHaveSecondaryElements {

	public const string Name = "Shifting Memory Of Ages";

	public override string SpiritName => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[]{
		new SpecialRule("Long Ages of Knowledge and Forgetfulness","When you would Forget a Power Card from your hand, you may instead discard it."),
		new SpecialRule("Insights Into the World's Nature","Some of your Actions let you Prepare Element Markers, which are kept here until used.  Choose the Elements freely.  Each Element Marker spent grants 1 of that Element for a single Action.")
	};

	static Track Prepare(int energy){
		Track t = Track.MkEnergy(energy);
		t.Action = new PrepareElement( $"{energy} energy" );
		t.Icon.Sub = new IconDescriptor { BackgroundImg = Img.ShiftingMemory_PrepareEl };
		return t;
	}

	static Track DiscardElementsForCardPlay => new Track("discard 2 elements for card play" ) { 
		Action = new DiscardElementsForCardPlay(2),
		Icon = new IconDescriptor { BackgroundImg = Img.ShiftingMemory_Discard2Prep },
	};

	CountDictionary<Element> IHaveSecondaryElements.SecondaryElements => PreparedElements;

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
			InnatePower.For(typeof(LearnTheInvadersTactics)), 
			InnatePower.For(typeof(ObserveTheEverChangingWorld))
		];
	}

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

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Put 2 presence on your starting board in the highest-number land that is Sands or Mountain.
		var space = board.Spaces.Last( x => x.IsOneOf( Terrain.Sands, Terrain.Mountain ) ).ScopeSpace;
		space.Setup(Presence.Token, 2);

		// Prepare 1 moon, 1 air, and 1 earth marker. (++ allows us to use SMOA for testing, where =1 overwrites testing values)
		PreparedElements[Element.Moon]++;
		PreparedElements[Element.Air]++;
		PreparedElements[Element.Earth]++;
	}






	public async Task PrepareElement(string context) {
		// This is only used by Shifting Memories
		var el = await this.SelectElementEx($"Prepare Element ({context})", ElementList.AllElements);
		PreparedElements[el]++;
	}

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



	public override bool CouldHaveElements( CountDictionary<Element> subset ) {
		var els = PreparedElements.Count != 0
			? Elements.Elements.Union(PreparedElements) 
			: Elements.Elements;
		return els.Contains(subset);
	}

	public readonly CountDictionary<Element> PreparedElements = [];

	/// <remarks> Override to present user with all options at 1 time and let them pick once. </remarks>
	public override async Task<IDrawableInnateTier> SelectInnateTierToActivate(IEnumerable<IDrawableInnateTier> innateOptions) {

		var options = innateOptions
			.Select(innateOption=>{
				var missing = innateOption.Elements.Except(Elements.Elements);
				return new { 
					innateOption, 
					missing,
					prompt = FormatPrompt(missing, innateOption.Elements)
				};
			})
			.Where( x => PreparedElements.Contains(x.missing) )
			.ToArray();

		static string FormatPrompt(CountDictionary<Element> missing, CountDictionary<Element> optionEls) {
			string optionElsString = ElementStrings.BuildElementString(optionEls);
			return missing.Count == 0
				? $"Use existing => {optionElsString}"
				: $"Prepare {ElementStrings.BuildElementString(missing)} => {optionElsString}";
		}

		if (options.Length == 0) return null;
		if(options.Length == 1) return options[0].innateOption;

		string choice = await this.SelectText("Select Innate Option", options.Select(x=>x.prompt).ToArray(), Present.Done);
		var match = options.FirstOrDefault(o=>o.prompt == choice);
		if(match == null) return null;

		Elements.Add(match.missing); // assign to this action so next check recognizes them
		foreach (var pair in match.missing)
			PreparedElements[pair.Key] -= pair.Value;
		
		return match.innateOption;
	}

	public override async Task<bool> HasElement( CountDictionary<Element> subset, string description ) {
		CountDictionary<Element> missing = subset.Except(Elements.Elements);
		if( missing.Count == 0 ) return true;

		// Check if we have prepared element markers to fill the missing elements
		if (PreparedElements.Count == 0) return false;

		string prompt = $"Meet elemental threshold: " + subset.BuildElementString();
		if( !PreparedElements.Contains(missing) 
			|| !await this.UserSelectsFirstText(prompt, $"Yes, use {ElementStrings.BuildElementString(missing)} prepared elements", "No, I'll pass.") 
		) return false;

		Elements.Add(missing); // assign to this action so next check recognizes them
		foreach (var pair in missing)
			PreparedElements[pair.Key] -= pair.Value;

		ActionScope.Current.AtEndOfThisAction(_ => {
			foreach (var pair in missing)
				Elements.Remove(pair.Key, pair.Value);
		});

		return true;

	}

	protected override object CustomMementoValue { 
		get => PreparedElements.ToArray();
		set => InitFromArray( PreparedElements, (KeyValuePair<Element, int>[])value );
	}

}