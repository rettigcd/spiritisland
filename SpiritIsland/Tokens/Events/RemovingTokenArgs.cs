namespace SpiritIsland;

public class RemovingTokenArgs {

	public Token Token { get; set; }
	public Space Space { get; set; }
	public int Count {
		get { return _count; }
		set { 
			// !!! something is making this negative
			if(value<0) throw new ArgumentOutOfRangeException(nameof(value),value,"Removing Token Args cannot be < 0");
			_count = value;
		}
	}
	int _count;
	public RemoveReason Reason { get; set; }
	public UnitOfWork ActionId { get; set; }
}
