namespace SpiritIsland;

public interface IPowerRepeater {
	Task<bool> ShouldRepeat( Spirit spirit, UnitOfWork actionScope );
}

