using SpiritIsland.JaggedEarth;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	// new spirit Painter each time layout / size changes
	class SpiritPainter : IDisposable {

		readonly Spirit _spirit;
		readonly PresenceTokenAppearance _presenceColor; // stored here because the presencePainter gets .Dispose() and needs it
		SpiritLayout spiritLayout;

		InnatePainter[] innatePainters;
		PresenceTrackPainter presencePainter;
		GrowthPainter growthPainter;

		public SpiritPainter(Spirit spirit, PresenceTokenAppearance presenceColor ) {
			_spirit = spirit;
			_presenceColor = presenceColor;
		}

		public void SetLayout( SpiritLayout layout ) {
			this.Dispose();

			this.spiritLayout = layout;
			growthPainter = new GrowthPainter( spiritLayout.growthLayout );
			presencePainter = new PresenceTrackPainter( _spirit, spiritLayout.trackLayout, _presenceColor );
			innatePainters = _spirit.InnatePowers
				.Select( power => new InnatePainter( power, spiritLayout.findLayoutByPower[power] ) )
				.ToArray();
		}

		public void Paint( Graphics graphics, GrowthActionFactory[] selectableGrowthActions ) {
			using var imageDrawer = new CachedImageDrawer();

			DrawSpiritImage( graphics );

			using(new StopWatch("Growth"))
				growthPainter.Paint( graphics, selectableGrowthActions );

			using(new StopWatch("Presence"))
				presencePainter.Paint( graphics, imageDrawer );

			using(new StopWatch( "Innates" ))
				Draw_Innates( graphics, imageDrawer );

			Draw_Elements( graphics );
		}

		void Draw_Innates( Graphics graphics, CachedImageDrawer imageDrawer ) {
			foreach(var painter in innatePainters)
				painter.DrawFromLayout( graphics, imageDrawer );
		}

		void DrawSpiritImage( Graphics graphics ) {
			Rectangle bounds = spiritLayout.imgBounds;
			var image = spiritImage ??= ResourceImages.Singleton.LoadSpiritImage(_spirit.Text);
			graphics.DrawImageFitBoth(image,bounds);
		}

		void Draw_Elements( Graphics graphics ) {
			// activated elements
			DrawActivatedElements( graphics, _spirit.Elements, spiritLayout.Elements );
			int skip = _spirit.Elements.Keys.Count; 
			if(skip>1) skip++; // add a space
			if(_spirit is IHaveSecondaryElements hasSecondaryElements)
				DrawActivatedElements( graphics, hasSecondaryElements.SecondaryElements, spiritLayout.Elements, skip );
		}

		void DrawActivatedElements( Graphics graphics, ElementCounts elements, ElementLayout elLayout, int skip=0 ) {

			var orderedElements = elements.Keys.OrderBy( el => (int)el );
			int idx = skip;
			foreach(var element in orderedElements) {
				var rect = elLayout.Rect(idx++);
				graphics.DrawImage( GetElementImage( element ), rect );
				graphics.DrawCountIfHigherThan( rect, elements[element]);
			}
		}

		Image GetElementImage( Element element ) {

			if(!elementImages.ContainsKey( element )) {
				Image image = ResourceImages.Singleton.GetImage( element.GetTokenImg() );
				elementImages.Add( element, image );
			}
			return elementImages[element];
		}

		public void Dispose() {
			growthPainter?.Dispose();

			presencePainter?.Dispose();

			if(innatePainters is not null)
				foreach(var ip in innatePainters)
					ip.Dispose();
		}

		Image spiritImage;
		readonly Dictionary<Element, Image> elementImages = new();


	}

}
