namespace SpiritIsland;

//public interface IAppearOnScreen : Token {}

public interface IVisibleToken : IToken, IOption {
	Img Img { get; }
}