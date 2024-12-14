namespace SpiritIsland.JaggedEarth;

class InsightsIntoTheWorldsNature(Spirit spirit) : ElementMgr(spirit) {

	public const string Name = "Insights Into the World's Nature";
	const string Description = "Some of your Actions let you Prepare Element Markers, which are kept here until used.  Choose the Elements freely.  Each Element Marker spent grants 1 of that Element for a single Action.";
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	public readonly CountDictionary<Element> PreparedElements = [];

	public override ECouldHaveElements CouldHave(CountDictionary<Element> subset) {
		CountDictionary<Element> missing = subset.Except(Elements);
		return missing.Count == 0 ? ECouldHaveElements.Yes
			: PreparedElements.Contains(missing) ? ECouldHaveElements.AsPrepared
			: ECouldHaveElements.No;
	}

	public override async Task<bool> MeetThreshold(CountDictionary<Element> subset, string description ) {

		// Ignores permission parameter.  If it requires using prepared elements, always asks.

		CountDictionary<Element> missing = subset.Except(Elements);
		if( missing.Count == 0 ) return true;

		// Check if we have prepared element markers to fill the missing elements
		if( PreparedElements.Count == 0 ) return false;

		string prompt = $"Meet elemental threshold: " + subset.BuildElementString();
		if( !PreparedElements.Contains(missing)
			|| !await _spirit.UserSelectsFirstText(prompt, $"Yes, use {ElementStrings.BuildElementString(missing)} prepared elements", "No, I'll pass.")
		) return false;

		PreparedElements.RemoveRange(missing); // remove from prepared
		Add(missing);  // add to Current
		ActionScope.Current.AtEndOfThisAction(_ => Remove(missing)); // remove from Current

		return true;
	}

	public override async Task<int> CommitToCount(Element singleElement) {
		int count = await base.CommitToCount(singleElement);
		int prepaired = PreparedElements[singleElement];
		if( prepaired == 0 ) return count;

		int userSelected = await _spirit.SelectNumber($"Use prepaired to boost {singleElement} to", prepaired+count, count);
		int numToUse = userSelected - count;
		PreparedElements[singleElement] -= numToUse;
		Elements[singleElement] += numToUse;
		ActionScope.Current.AtEndOfThisAction(_ => Elements[singleElement] -= numToUse); // remove from Current
		return userSelected;
	}

}
