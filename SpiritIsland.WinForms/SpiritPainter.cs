using SpiritIsland.JaggedEarth;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	class SpiritPainter {

		readonly Spirit spirit;

		public SpiritPainter(Spirit spirit ) {
			this.spirit = spirit;
		}

		public void Paint( Graphics graphics, SpiritLayout spiritLayout, 
			InnatePower[] innateOptions, 
			GrowthOption[] selectableGrowthOptions,
			GrowthActionFactory[] selectableGrowthActions,
			Track[] clickableTrackOptions,
			string presenceColor
		) {
			DrawSpiritImage( graphics, spiritLayout.imgBounds );

			new GrowthPainter( graphics ).Paint( spiritLayout.growthLayout, selectableGrowthOptions, selectableGrowthActions );
			new PresenceTrackPainter( spirit, spiritLayout.trackLayout, clickableTrackOptions, graphics ).Paint( presenceColor );
			Draw_Innates( graphics, spiritLayout, innateOptions );
			Draw_Elements( graphics, spiritLayout );
		}

		void Draw_Innates( Graphics graphics, SpiritLayout spiritLayout, InnatePower[] innateOptions ) {
			using var innatePainter = new InnatePainter( graphics );
			foreach(var power in spirit.InnatePowers)
				innatePainter.DrawFromMetrics( power, spiritLayout.innateLayouts[power], spirit.Elements, innateOptions.Contains(power));
		}

		void DrawSpiritImage( Graphics graphics, Rectangle bounds ) {
			var image = spiritImage ??= LoadSpiritImage();
			graphics.DrawImageFitBoth(image,bounds);
		}

		Image LoadSpiritImage() {
			string filename = spirit.Text.Replace( ' ', '_' );
			return Image.FromFile( $".\\images\\spirits\\{filename}.jpg" );
		}

		void Draw_Elements( Graphics graphics, SpiritLayout spiritLayout ) {
			// activated elements
			DrawActivatedElements( graphics, spirit.Elements, spiritLayout.Elements );
			int skip = spirit.Elements.Keys.Count; 
			if(skip>1) skip++; // add a space
			if(spirit is ShiftingMemoryOfAges smoa)
				DrawActivatedElements( graphics, smoa.PreparedElements, spiritLayout.Elements, skip );
		}


		void DrawActivatedElements( Graphics graphics, CountDictionary<Element> elements, ElementLayout elLayout, int skip=0 ) {

			var orderedElements = elements.Keys.OrderBy( el => (int)el );
			int idx = skip;
			foreach(var element in orderedElements) {
				var rect = elLayout.Rect(idx++);
				graphics.DrawImage( GetElementImage( element ), rect );
				graphics.DrawCount( rect, elements[element]);
			}
		}

		Image GetElementImage( Element element ) {

			if(!elementImages.ContainsKey( element )) {
				Image image = ResourceImages.Singleton.GetToken( element );
				elementImages.Add( element, image );
			}
			return elementImages[element];
		}

		Image spiritImage;
		readonly Dictionary<Element, Image> elementImages = new();


	}

}
