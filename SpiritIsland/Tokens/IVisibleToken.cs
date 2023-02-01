namespace SpiritIsland;

//public interface IAppearOnScreen : Token {}

public interface IVisibleToken : IToken {
	Img Img { get; }
}