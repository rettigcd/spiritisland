using SpiritIsland.JaggedEarth;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

/// <summary>
/// Binds together for a single painting: Graphics, Spirit(info), Layout, Clickable-Options
/// </summary>
class PresenceTrackPainter  {

	public static IPaintableRect GetPaintablePresenceTracks( ClickableContainer cc, SharedCtx ctx ){

		var presence = ctx._spirit.Presence;
		
		return new RowRect(
			new StackedRowRect( 
				Label("Energy", BuildSlots(presence.Energy) ),
				Label("Card Plays", BuildSlots(presence.CardPlays ))
			), 
			TheRightStuff( cc, ctx )
		);

		RowRect BuildSlots(IPresenceTrack track) => new RowRect([..track.Slots.Select(slot=>new SlotRect(slot,track,cc,ctx))]);

	}

	static PoolRect TheRightStuff( ClickableContainer cc, SharedCtx ctx ){
		var spirit = ctx._spirit;
		ImageSpec presence = ImageSpec.From( ctx._spirit.Presence.Token );

		var pool = new PoolRect(){ WidthRatio = .8f }
			.Float(CurrentEnergyRect(cc,spirit),.3f,0, .7f,.7f )
			.Float( Destroyed( presence, spirit ).ShowIf(()=>0<spirit.Presence.Destroyed.Count ), .60f,.65f,.4f,.3f );

		if(spirit is FracturedDaysSplitTheSky days)
			pool.Float( Time( presence, days.Time), .2f,.65f,.3f,.3f );

		return pool;
	}

	static PoolRect CurrentEnergyRect(ClickableContainer cc,Spirit spirit){
		return new PoolRect()
			.Float(new ImgRect(Img.Coin))
			.Float(new TextRect(()=>spirit.Energy.ToString()).RiseAbove(cc.PaintAboves),0f,.25f,1f,.6f);
	}

	static PoolRect Destroyed( ImageSpec presence, Spirit spirit ){

		string MakeSubscript(){
			int destroyed = spirit.Presence.Destroyed.Count;
			return (destroyed==0) ? string.Empty
				: $"x{destroyed}";
		}

		return new PoolRect{WidthRatio=1.2f}
			.Float(new ImgRect(presence))
			.Float(new ImgRect(Img.DestroyedX))
			.Float(new SubScriptRect( MakeSubscript ).ShowIf(()=>1<spirit.Presence.Destroyed.Count));
	}

	static PoolRect Time( ImageSpec presence, int time ){
		var pool = new PoolRect()
			.Float(new ImgRect( presence ))
			.Float(new ImgRect(Img.Hourglass), 0.5f,0,.5f,.5f);
		if(1<time)
			pool.Float(new SubScriptRect(time.ToString()));
		return pool;
	}

	static PoolRect Label( string label, IPaintableRect child ){
		return new PoolRect(){ WidthRatio = child.WidthRatio }
			.Float(child)
			.Float(new TextRect(label){ Horizontal = StringAlignment.Near},0,0,.25f,.25f);
	}

}
