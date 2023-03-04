using System.Drawing;

namespace SpiritIsland.WinForms;

public class PresenceSlotLayout {

	public Rectangle PresenceRect;
	public RectangleF TrackRect;
	public Rectangle DebugBounds;

}

//public class DestroyedPresenceBtn : IButton {

//	public SlotMetrics( SpiritPresence presence, Track track, Image presenceImage ) {
//		_presenceTrack = presenceTrack;
//		_track = track;
//		_presenceImage = presenceImage;
//	}

//	public Rectangle Bounds => throw new System.NotImplementedException();

//	public virtual void Paint( Graphics graphics, bool enabled ) {
//		if(enabled) {
//			using Pen highlightPen = new( Color.Red, 8f );
//			graphics.DrawEllipse( highlightPen, PresenceRect );
//		}

//		if(!_presenceTrack.Revealed.Contains( _track ))
//			graphics.DrawImage( _presenceImage, PresenceRect );
//	}


//}
