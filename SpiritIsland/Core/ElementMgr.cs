namespace SpiritIsland;

/// <summary>
/// Manages current elements assigned to a Spirit
/// </summary>
public class ElementMgr( Spirit _spirit ) {
	public CountDictionary<Element> Elements { get; set; } = [];

	public int this[Element el] {
		set { Elements[el] = value; }
	}

	#region Set (Init,Add,Remove)

	/// <remarks> Used during </remarks>
	public void Init( CountDictionary<Element> elements ) {
		Elements.Clear();
		foreach(KeyValuePair<Element, int> pair in elements)
			Elements[pair.Key] += pair.Value;
	}

	/// <remarks>Adding elements from played power cards.</remarks>
	public void Add( Element el, int count ) { Elements[el] += count; }

	/// <remarks>From Growth Option or when Track-Slot is revealed.</remarks>
	public void Add( params Element[] els ) { Elements.AddRange(els); }

	/// <remarks> Prepared Elements </remarks>
	public void Add( CountDictionary<Element> counts ) { Elements.AddRange(counts); }

	public void Remove(Element el, int count) { Elements[el] -= count; }

	#endregion Set (Init,Add,Remove)

	/// <summary> Checks all elements that are available to spirit. </summary>
	public virtual bool CouldContain( CountDictionary<Element> subset ) {

		CountDictionary<Element> missing = subset.Except( Elements );

		if(0 < missing.Total)
			foreach(Element multiElement in Elements.GetMultiElements())
				CouldApplyMultiToMissing( missing, multiElement );

		return missing.Total == 0;

	}

	#region Could Have Elments (helpers)

	// Helps test if we COULD have enough if we committed a multi-element to one of its parts
	void CouldApplyMultiToMissing( CountDictionary<Element> missing, Element wildEl ) {
		int wilds = Elements[wildEl];
		foreach(Element el in wildEl.SplitIntoSingles()) {
			int useAsEl1 = Math.Min( missing[el], wilds );
			missing[el] -= useAsEl1;
			wilds -= useAsEl1;
		}
	}

	#endregion Could Have Elments (helpers)

	/// <summary> 
	/// Gets current Element count WITHOUT allowing spirit to convert .Any or Multi-Element
	/// </summary>
	public int Get( Element el ) => Elements[el];

	/// <summary>
	/// Checks elements As-Is. Does not allow Spirit to Realize .Any or any of the Multi-Element.
	/// </summary>
	public bool Contains( CountDictionary<Element> subset )
		=> subset.All( pair => pair.Value <= Elements[pair.Key] );

	public async Task<int> GetAsync( Element single ) {
		foreach(Element multi in Elements.GetMultiElements().Where(m=>m.HasSingle(single))) {
			int count = Elements[multi];

			while(0 < count--) { 
				var options = multi.SplitIntoSingles();
				string multiStr = GetMultiStr( multi );
				An.ElementOption selection = await _spirit.SelectAsync( new An.Element( $"Select element from {multiStr}?", options, Present.Done, single ) );
				Element pick = selection is ItemOption<Element> el ? el.Item : Element.None;
				if(pick == Element.None) break;

				// Convert from Multi to Single
				Elements[multi]--;
				Elements[single]++;
			}

			//string multiStr = GetMultiStr( multi );
			//if(await _spirit.UserSelectsFirstText( $"Use {multiStr} as {single}?", $"Yes, use {count} as {single}", "No thanks" )) {
			//	// Convert from Multi to Single
			//	Elements[multi] -= count;
			//	Elements[single] += count;
			//}

		}
		return Elements[single];
	}

	/// <summary>
	/// Checks elements available, and commits them (like the 'Any' element)
	/// </summary>
	public virtual async Task<bool> ContainsAsync( CountDictionary<Element> subset, string usageDescription ) {

		// For normal spirits without Prepared Elements, this is the same as Could Have Elements
		CountDictionary<Element> missing = subset.Except( Elements );
		if(0 < missing.Count)
			foreach(Element multi in Elements.GetMultiElements())
				await ApplyMultiElementsToMissing( usageDescription, missing, multi );
		return missing.Count == 0;
	}

	#region ContainsAsync Helpers

	async Task ApplyMultiElementsToMissing( string usageDescriptoin, CountDictionary<Element> missing, Element multi ) {
		foreach(Element single in multi.SplitIntoSingles())
			await ApplyElementToMissing( usageDescriptoin, missing, multi, single );
	}

	async Task ApplyElementToMissing( string usageDescription, CountDictionary<Element> missing, Element multi, Element single ) {
		int count = Math.Min( missing[single], Elements[multi] );
		while(0 < count--) { 
			var options = multi.SplitIntoSingles();
			string multiStr = GetMultiStr( multi );
			An.ElementOption selection = await _spirit.SelectAsync( new An.Element( $"Select element from {multiStr} for {usageDescription}?", options, Present.Done, single ) );
			Element pick = selection is ItemOption<Element> el ? el.Item : Element.None;
			if(pick == Element.None) break;

			// Convert from Multi to Single
			Elements[multi]--;
			Elements[single]++;
			// no-longer missing
			missing[single]--;
		}

	}

	static string GetMultiStr( Element multi ) => multi.SplitIntoSingles().Select( x => x.ToString() ).Join( "/" );

	#endregion ContainsAsync Helpers

	public string Summary(bool showOneCount=true) => Elements.BuildElementString(showOneCount);

}