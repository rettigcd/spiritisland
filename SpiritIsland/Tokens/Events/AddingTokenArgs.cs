namespace SpiritIsland;

public class AddingTokenArgs {

	public AddingTokenArgs( IToken token, int count, Space to, AddReason addReason ) {
		Token = token;
		Count = count;
		To = to;
		Reason = addReason;
	}

	public Space To { get; }
	public AddReason Reason { get; }

	// Modifiable
	public IToken Token { get; set; }
	public int Count {
		get => _count;
		set {  
			if(_count != value && Reason == AddReason.MovedTo) 
				throw new InvalidOperationException($"Changing {nameof(Count)} not allowed for {nameof(AddReason.MovedTo)}.");
			_count = value;
		}
	}
	int _count;
}
