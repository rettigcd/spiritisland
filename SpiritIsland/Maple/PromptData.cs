namespace SpiritIsland;

public class PromptData( IQuota quota, SpaceToken[] options, int index, int? maxCount = 0 ) {

	public readonly int Index = index;
	public readonly int? MaxCount = maxCount;
	public int RemainingCount => MaxCount.HasValue ? (MaxCount.Value - Index) : int.MaxValue;

	public string RemainingPartsStr => quota.RemainingTokenDescriptionOn( options.Select( st => st.Space ).Distinct().ToArray() );
}

