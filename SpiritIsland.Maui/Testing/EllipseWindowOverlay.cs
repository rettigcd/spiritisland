//using Android.OS;
namespace SpiritIsland.Maui;

public class EllipseWindowOverlay : IWindowOverlayElement {

	readonly Rect _rect = new Rect(0, 0, 300, 200);

	public bool Contains(Point point) {
		return _rect.Contains(point);
	}

	public void Draw(ICanvas canvas, RectF dirtyRect) {
		canvas.FillColor = Colors.Pink;
		canvas.FillEllipse(_rect);
	}
}