using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

/// <summary>
/// 1) Manages static / dynamic clickable
/// 2) Maintains list of what Paints Dynamic content above the main.
/// </summary>
public class ClickableContainer {

	#region Clickable

	public int OptionCount => _optionCount;

	public void AddStatic( IClickableLocation clickable ){
		ArgumentNullException.ThrowIfNull( clickable );

		// Static clickables go at beginning of List
		_clickables.Insert(_staticCount++, clickable);
		if( clickable is IPaintAbove above)
			PaintAboves.Add( above );
	}

	/// <summary>
	/// Makes the ClickableLocation clickable whenever the Option is part of the decision.
	/// </summary>
	public void RegisterOption( IOption option, IClickableLocation clickable){
		ArgumentNullException.ThrowIfNull( clickable );
		_lookupOptionClickable[option] = clickable;
		if( clickable is IPaintAbove above)
			PaintAboves.Add( above );
	}

	public void ActivateOptions( IDecision decision ) {
		// remove old
		for(int i=_staticCount;i<_clickables.Count;++i)
			if(_clickables[i] is IAmEnablable enablable)
				enablable.Enabled = false;
		_clickables.RemoveRange(_staticCount,_clickables.Count-_staticCount);

		// Add new
		_optionCount = 0;
		foreach(var option in decision.Options){
			if(_lookupOptionClickable.TryGetValue(option, out IClickableLocation clickable)){
				_clickables.Add(clickable);
				++_optionCount;
				if( clickable is IAmEnablable enablable )
					enablable.Enabled = true;
			}
		}

	}

	public IClickable GetClickableAt( Point clientCoords ) 
		=> _clickables.FirstOrDefault(c=>c.Contains(clientCoords));

	public void ClearAllClickables(){
		_clickables.Clear();
		_staticCount = 0;
		_lookupOptionClickable.Clear();
		PaintAboves.Clear();
	}

	#endregion Clickable

	public void PaintAbove(Graphics graphics){
		foreach(var above in PaintAboves) above.PaintAbove(graphics);
	}

	public List<IPaintAbove> PaintAboves = [];

	#region private

	int _optionCount;
	int _staticCount;

	// Only holds items that can be clicked.  Non-enabled Options do NOT go in here.
	readonly List<IClickableLocation> _clickables = [];

	readonly Dictionary<IOption,IClickableLocation> _lookupOptionClickable = [];

	#endregion

}

class SlotRect : IPaintableRect, IPaintAbove, IAmEnablable, IClickableLocation {

	readonly Track _slot;
	readonly IPresenceTrack _track;
	readonly SharedCtx _ctx;
	IOption _revealOption;
	IOption _coverOption;
	Bitmap _tokenImg;

	public SlotRect(Track slot, IPresenceTrack track, ClickableContainer clickableContainer, SharedCtx ctx ){
		_slot = slot;
		_track = track;
		_ctx = ctx;
		_tokenImg = (Bitmap)ctx._imgCache.AccessTokenImage(ctx._spirit.Presence.Token);
		_background = new PoolRect{ WidthRatio = .8f }
			.Float(_presenceBounds,.1f,0,.8f,.6f)
			.Float(new ImgRect( slot.Icon ), .1f, .3f, .8f, .7f );

		// Register for removing presence from track
		_revealOption = new TrackPresence(slot,ctx._spirit.Presence.Token);
		clickableContainer.RegisterOption(_revealOption,this);
		// Register returning presence to the track
		_coverOption = slot;
		clickableContainer.RegisterOption(_coverOption,this);
	}

	public float? WidthRatio => _background.WidthRatio;

	public bool Enabled { get; set; }

	public void Paint( Graphics graphics, Rectangle bounds ){
		var fitted = bounds.FitBoth(WidthRatio!.Value);
		_background.Paint(graphics,fitted);
	}

	void IPaintAbove.PaintAbove( Graphics graphics ){
		if(Enabled) {
			using Pen highlightPen = new( Color.Red, 8f );
			graphics.DrawEllipse( highlightPen, _presenceBounds.Bounds );
		}

		if(!_track.Revealed.Contains( _slot ))
			new ImgRect(_tokenImg).Paint(graphics,_presenceBounds.Bounds);
	}

	bool HasPresence => !_track.Revealed.Contains( _slot );

	public void Click(){
		_ctx.SelectOption( HasPresence ? _revealOption : _coverOption );
	}
	public bool Contains( Point point ) => _presenceBounds.Bounds.Contains(point);

	BoundsCapture _presenceBounds =  new BoundsCapture();
	PoolRect _background;
}

class BoundsCapture : IPaintableRect {
	public Rectangle Bounds { get; private set;}
	public float? WidthRatio {get; set;}
	public void Paint( Graphics graphics, Rectangle bounds ){
		Bounds = bounds;
	}
}