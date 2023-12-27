using System;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

public class PresenceSlotButton : PresenceSlotLayout, IButton {

	public PresenceSlotButton( IPresenceTrack presenceTrack, Track track, Image presenceImage ) {
		_presenceTrack = presenceTrack;
		_track = track;
		_presenceImage = presenceImage ?? throw new ArgumentNullException(nameof(presenceImage));
	}

	readonly Track _track;
	readonly Image _presenceImage;
	readonly IPresenceTrack _presenceTrack;

	public Rectangle Bounds => PresenceRect;
	bool IButton.Contains( Point clientCoords) => Bounds.Contains( clientCoords );

	public void PaintBackground( Graphics graphics ) {
		using Image img = ResourceImages.Singleton.GetTrackSlot( _track.Icon );
		graphics.DrawImage( img, TrackRect );
	}

	public virtual void Paint( Graphics graphics, bool enabled ) {
		if(enabled) {
			using Pen highlightPen = new( Color.Red, 8f );
			graphics.DrawEllipse( highlightPen, PresenceRect );
		}

		if(!_presenceTrack.Revealed.Contains( _track ))
			graphics.DrawImage( _presenceImage, PresenceRect );
	}

	void IButton.SyncDataToDecision( IDecision _ ) { }
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
