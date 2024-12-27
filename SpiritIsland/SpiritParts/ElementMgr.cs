#nullable enable
namespace SpiritIsland;

/// <summary>
/// Manages current elements assigned to a Spirit
/// </summary>
public class ElementMgr( Spirit spirit ) {

	/// <summary> Syntax Sugar - Used inside Power Cards </summary>
	public Task<bool> YouHave(string elementString)
		=> AskToMeetThreshold(ElementStrings.Parse(elementString), "Power Card Threshold" );

	/// <summary> Checks with user to see if they wish to meet the given threshold. </summary>
	public async Task<bool> AskToMeetThreshold( CountDictionary<Element> subset, string usageDescription ) {
		return CouldHave(subset) != ECouldHaveElements.No
			&& await _spirit.UserSelectsFirstText("Activate Element Threshold?", "Yes", "No")
			&& await MeetThreshold(subset, usageDescription);
	}

	/// <summary> Attempts to meet the listed threshold. </summary>
	/// <remarks> Assigns Any elements, picks between 2 elements, for Shifting Memories, uses prepaired elements </remarks>
	public virtual async Task<bool> MeetThreshold(CountDictionary<Element> subset, string usageDescription) {

		// For normal spirits without Prepared Elements, this is the same as Could Have Elements
		CountDictionary<Element> missing = subset.Except(Elements);
		if( 0 < missing.Count )
			foreach( Element multi in Elements.GetMultiElements() )
				await ApplyMultiElementsToMissing(usageDescription, missing, multi);
		return missing.Count == 0;

		async Task ApplyMultiElementsToMissing(string usageDescriptoin, CountDictionary<Element> missing, Element multi) {
			foreach( Element single in multi.SplitIntoSingles() )
				await ApplyElementToMissing(usageDescriptoin, missing, multi, single);
		}

		async Task ApplyElementToMissing(string usageDescription, CountDictionary<Element> missing, Element multi, Element single) {
			int count = Math.Min(missing[single], Elements[multi]);
			while( 0 < count-- ) {
				var options = multi.SplitIntoSingles();
				string multiStr = GetMultiStr(multi);
				An.ElementOption? selection = await _spirit.Select(new An.Element($"Select element from {multiStr} for {usageDescription}?", options, Present.Done, single));
				Element pick = selection is ItemOption<Element> el ? el.Item : Element.None;
				if( pick == Element.None ) break;

				// Convert from Multi to Single
				Elements[multi]--;
				Elements[single]++;
				// no-longer missing
				missing[single]--;
			}
		}
	}

	/// <summary>
	/// Spirit has enough ANYs, Prepaired, choice, that they *COULD* meet the threshold if they chose.
	/// </summary>
	/// <param name="subset"></param>
	/// <returns></returns>
	public virtual ECouldHaveElements CouldHave(CountDictionary<Element> subset) {
		CountDictionary<Element> missing = subset.Except(Elements);

		if( 0 < missing.Total )
			foreach( Element multiElement in Elements.GetMultiElements() )
				CouldApplyMultiToMissing(missing, multiElement);

		return missing.Total == 0 ? ECouldHaveElements.Yes : ECouldHaveElements.No;

		// Helper - tests if we COULD have enough if we committed a multi-element to one of its parts
		void CouldApplyMultiToMissing(CountDictionary<Element> missing, Element wildEl) {
			int wilds = Elements[wildEl];
			foreach( Element el in wildEl.SplitIntoSingles() ) {
				int useAsEl1 = Math.Min(missing[el], wilds);
				missing[el] -= useAsEl1;
				wilds -= useAsEl1;
			}
		}

	}

	/// <summary>
	/// Makes user commit to a # of elements.
	/// </summary>
	public virtual async Task<int> CommitToCount( Element single ) {
		foreach(Element multi in Elements.GetMultiElements().Where(m=>m.HasSingle(single))) {
			int count = Elements[multi];

			while(0 < count--) { 
				var options = multi.SplitIntoSingles();
				string multiStr = GetMultiStr( multi );
				An.ElementOption? selection = await _spirit.Select( new An.Element( $"Select element from {multiStr}?", options, Present.Done, single ) );
				Element pick = selection is ItemOption<Element> el ? el.Item : Element.None;
				if(pick == Element.None) break;

				// Convert from Multi to Single
				Elements[multi]--;
				Elements[single]++;
			}
		}
		return Elements[single];
	}

	public virtual async Task<IDrawableInnateTier?> SelectInnateTierToActivate(IEnumerable<IDrawableInnateTier> innateOptions) {
		IDrawableInnateTier? match = null;
		foreach( var option in innateOptions.OrderBy(o => o.Elements.Total) )
			if( await _spirit.Elements.HasMetTierThreshold(option) )
				match = option;
		return match;
	}


	public virtual async Task<bool> HasMetTierThreshold(IDrawableInnateTier option) {
		return await MeetThreshold(option.Elements, "Innate Tier");
	}


	#region CommitToMany Helpers (private)

	static string GetMultiStr( Element multi ) => multi.SplitIntoSingles().Select( x => x.ToString() ).Join( "/" );

	#endregion CommitToMany Helpers (private)

	#region RAW values

	public CountDictionary<Element> Elements { get; set; } = [];

	// Gets/Sets Element count WITHOUT convert ANY or Multi-Element
	public int this[Element el] {
		get => Elements[el];
		set => Elements[el] = value;
	}

	/// <summary> Checks elements As-Is. Does not allow Spirit to Realize .Any or any of the Multi-Element. </summary>
	public bool ContainsRaw(CountDictionary<Element> subset)
		=> subset.All(pair => pair.Value <= Elements[pair.Key]);

	public string Summary(bool showOneCount = true) => Elements.BuildElementString(showOneCount);

	/// <remarks> Used during </remarks>
	public void Init(CountDictionary<Element> elements) {
		Elements.Clear();
		foreach( KeyValuePair<Element, int> pair in elements )
			Elements[pair.Key] += pair.Value;
	}

	/// <remarks>From Growth Option or when Track-Slot is revealed.</remarks>
	public void Add(params Element[] els) { Elements.AddRange(els); } // 7 uses

	/// <remarks> Prepared Elements </remarks>
	public void Add(CountDictionary<Element> counts) { Elements.AddRange(counts); } // 8 uses

	public void Remove(CountDictionary<Element> counts) { Elements.RemoveRange(counts); } // 2 use

	#endregion RAW values

	#region protected fields

	protected readonly Spirit _spirit = spirit;

	#endregion

}
