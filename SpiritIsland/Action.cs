
namespace SpiritIsland;

public class UnitOfWork {

	public Guid Id { get; }

	public UnitOfWork() {
		Id = Guid.NewGuid();
	}

}

