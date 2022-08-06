namespace SpiritIsland;

public class AddingTokenArgs {

	public Token Token { get; set; }
	public Space Space { get; set; }
	public int Count {
		get { return _count; }
		set {
			// !!! something is making this negative
			if(value < 0) throw new ArgumentOutOfRangeException( nameof( value ), value, "Removing Token Args cannot be < 0" );
			_count = value;
		}
	}
	int _count;
	public AddReason Reason { get; set; }
	public Guid ActionId { get; set; }
}
