namespace SpiritIsland;

public static class ElementList {

	public static readonly Element[] AllElements = new Element[] { Element.Sun, Element.Moon, Element.Fire, Element.Air, Element.Water, Element.Earth, Element.Plant, Element.Animal };

	static public Img GetTokenImg(this Element element) => element switch {
		Element.Sun    => Img.Token_Sun,
		Element.Moon   => Img.Token_Moon,
		Element.Air    => Img.Token_Air,
		Element.Fire   => Img.Token_Fire,
		Element.Water  => Img.Token_Water,
		Element.Plant  => Img.Token_Plant,
		Element.Earth  => Img.Token_Earth,
		Element.Animal => Img.Token_Animal,
		Element.Any	   => Img.Token_Any,
		_              => throw new ArgumentOutOfRangeException(nameof(element)),
	};

	static public Img GetIconImg(this Element element) => element switch {
		Element.Sun    => Img.Icon_Sun,
		Element.Moon   => Img.Icon_Moon,
		Element.Air    => Img.Icon_Air,
		Element.Fire   => Img.Icon_Fire,
		Element.Water  => Img.Icon_Water,
		Element.Plant  => Img.Icon_Plant,
		Element.Earth  => Img.Icon_Earth,
		Element.Animal => Img.Icon_Animal,
		_              => throw new ArgumentOutOfRangeException(nameof(element)),
	};

}