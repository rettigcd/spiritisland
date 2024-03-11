namespace SpiritIsland;

public class RemovingTokenArgs {

	public RemovingTokenArgs( Space from, RemoveReason reason ) {
		if(reason == DestroyingFromDamage.TriggerReason)
			throw new ArgumentException("Do not .TriggerReason in event args", nameof( reason ) );
		From = from;
		Reason = reason;
	}

	// Read-only
	public Space From { get; }
	public RemoveReason Reason { get; }

	// modifiable
	public IToken Token { get; set; }

	// Should never be negative.
	public int Count {
		get { return _count; }
		set { 
			if( value < 0 ) throw new ArgumentOutOfRangeException(nameof(Count),"Removing Count cannot be < 0");
			_count = value;
		}
	}
	int _count;
}