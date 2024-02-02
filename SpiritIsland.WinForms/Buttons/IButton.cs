using System;
using System.Drawing;

namespace SpiritIsland.WinForms;

public interface IAmEnablable { bool Enabled {set;} }

public interface IButton : IAmEnablable, IPaintAbove {
	bool Contains( Point clientCoords );
}

// Eventually merge this into IButton...
public interface IClickable {
	void Click();
}

public interface IClickableLocation : IClickable {
	bool Contains(Point point);
}

public class GenericClickable(Action action) : IClickable {

	public void Click() => action();
}
