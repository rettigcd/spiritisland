﻿using System.Drawing;

namespace SpiritIsland.WinForms {
	public class ElementLayout {
		public ElementLayout(Rectangle bounds ) {
			x = bounds.X;
			y = bounds.Y;
			elementSize = bounds.Height;
			step = elementSize + bounds.Height/10;
		}
		public Rectangle Rect(int index) => new Rectangle(x+step*index,y,elementSize,elementSize);

		readonly int x;
		readonly int y;
		readonly int step;
		readonly int elementSize;
	}


}
