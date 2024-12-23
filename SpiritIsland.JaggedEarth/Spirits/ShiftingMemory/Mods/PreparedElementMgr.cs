namespace SpiritIsland.JaggedEarth;

public class PreparedElementMgr(Spirit spirit) : ElementMgr(spirit) {

	public readonly CountDictionary<Element> PreparedElements = [];

	public Task Prepare(string context) {
		return Prepare(context, ElementList.AllElements);
	}

	public async Task Prepare(string context, Element[] elementOptions) {
		var el = await _spirit.SelectElementEx($"Prepare Element ({context})", elementOptions);
		PreparedElements[el]++;
	}

	public async Task<CountDictionary<Element>> DiscardElements(int totalNumToRemove, string effect) {
		var discarded = new CountDictionary<Element>();

		int index = 0;
		while( index++ < totalNumToRemove ) {
			Element el = await _spirit.SelectElementEx($"Select element to discard for {effect} ({index} of {totalNumToRemove})", PreparedElements.Keys, Present.Done);
			if( el == default ) break;
			PreparedElements[el]--;
			discarded[el]++;
		}
		return discarded;
	}

}


public class PrepareElement(string context) : SpiritAction("PrepareElement") {

	static public Track MakeTrack(int energy) {
		return new Track(energy + " energy, prepare-El") {
			Energy = energy,
			Icon = new IconDescriptor {
				BackgroundImg = Img.Coin,
				Text = energy.ToString(),
				Sub = new IconDescriptor { BackgroundImg = Img.ShiftingMemory_PrepareEl }
			},
			Action = new PrepareElement($"{energy} energy"),
		};
	}


	public override async Task ActAsync(Spirit self) {
		if( self is ShiftingMemoryOfAges smoa )
			await smoa.PreparedElementMgr.Prepare(context);
	}
}

public class DiscardElementsForCardPlay(int elementDiscardCount) : SpiritAction("DiscardElementsForCardPlay") {

	static public Track MakeTrack(int count) => new Track($"discard {count} elements for card play") {
		Action = new DiscardElementsForCardPlay(count),
		Icon = new IconDescriptor { BackgroundImg = Img.ShiftingMemory_Discard2Prep },
	};

	// When revealing Discard-Elements-For-Extra-Card-Plays during non-growth (i.e. Unrelenting Growth), don't activate - it is too late to add card plays.

	readonly int _totalNumToRemove = elementDiscardCount;

	public override async Task ActAsync(Spirit self) {
		if( self is ShiftingMemoryOfAges smoa
			&& _totalNumToRemove <= smoa.PreparedElementMgr.PreparedElements.Total
			&& (await smoa.PreparedElementMgr.DiscardElements(_totalNumToRemove, "additional card-play")).Count == _totalNumToRemove
		) smoa.TempCardPlayBoost++;
	}

}