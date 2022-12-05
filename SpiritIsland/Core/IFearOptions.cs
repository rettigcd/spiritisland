namespace SpiritIsland;

public interface IFearOptions {
	string Name { get; }
	Task Level1( GameCtx ctx );
	Task Level2( GameCtx ctx );
	Task Level3( GameCtx ctx );
}
