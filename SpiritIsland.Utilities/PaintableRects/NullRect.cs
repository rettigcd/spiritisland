using System.Drawing;

namespace SpiritIsland;

public class NullRect : IPaintableBlockRect {
    public float WidthRatio {get;set;} = 1f;

    public Rectangle Paint( Graphics graphics, Rectangle rect ){ /* draw nothing */ return rect; }
}
