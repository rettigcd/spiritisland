namespace SpiritIsland.JaggedEarth;

class UserSelectedInnatePower(Type type) : InnatePower(type) {

	protected override async Task<IDrawableInnateTier?> SelectInnateTierToActivate(Spirit spirit, IEnumerable<IDrawableInnateTier> innateOptions) {

		// !!! THIS SHOULD BE Spirit AGNOSTIC and not know about Prepaired elements.
		var preparedMgr = (PreparedElementMgr)spirit.Elements;

		// Let user pick level
		var options = innateOptions
			.Select(innateOption => {
				var missing = innateOption.Elements.Except(spirit.Elements.Elements);
				return new {
					innateOption,
					missing,
					prompt = FormatPrompt(missing, innateOption.Elements)
				};
			})
			.Where(x => preparedMgr.PreparedElements.Contains(x.missing))
			.ToArray();

		if( options.Length == 0 ) return null;
		if( options.Length == 1 ) return options[0].innateOption;

		string? choice = await spirit.SelectText("Select Innate Option", options.Select(x => x.prompt).ToArray(), Present.Done);
		var match = options.FirstOrDefault(o => o.prompt == choice);
		if( match is null ) return null;

		// Assign to this action so next check recognizes them
		// (!!! BUG - should only be applied to this action, not all.  Remove at end of action)
		preparedMgr.Add(match.missing);
		foreach( var pair in match.missing )
			preparedMgr.PreparedElements[pair.Key] -= pair.Value;

		// return selected result
		return match.innateOption;

		static string FormatPrompt(CountDictionary<Element> missing, CountDictionary<Element> optionEls) {
			string optionElsString = ElementStrings.BuildElementString(optionEls);
			return missing.Count == 0
				? $"Use existing => {optionElsString}"
				: $"Prepare {ElementStrings.BuildElementString(missing)} => {optionElsString}";
		}

	}
}