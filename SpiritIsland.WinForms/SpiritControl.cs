using SpiritIsland.Core;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {

    public partial class SpiritControl : Control {

        Element[] Highest(InnatePower power ) => power.GetTriggerThresholds()
            .OrderByDescending(list=>list.Length)
            .First();

        public void Init(Spirit spirit, IHaveOptions optionProvider ) {
            this.spirit = spirit;

            var counts = spirit.InnatePowers
                .SelectMany(Highest)
                .GroupBy(x=>x)
                .ToDictionary(grp=>grp.Key,grp=>grp.Count());
            highestInnate = new CountDictionary<Element>(counts);

            optionProvider.OptionsChanged += OptionProvider_OptionsChanged;
        }

        CountDictionary<Element> highestInnate;
        readonly Dictionary<InnatePower,Image> innateImages = new();

        Image GetImage( InnatePower card ) {

            if(!innateImages.ContainsKey( card )) {
                string filename = card.Name.Replace( ' ', '_' ).Replace( "'", "" ).ToLower();
                Image image = Image.FromFile( $".\\images\\{filename}.jpg" );
                innateImages.Add( card, image );
            }
            return innateImages[card];
        }

        readonly Dictionary<Element,Image> elementImages = new();
        Image GetImage( Element element ) {

            if(!elementImages.ContainsKey( element )) {
                string filename = "Simple_"+element.ToString().ToLower();
                Image image = Image.FromFile( $".\\images\\{filename}.png" );
                elementImages.Add( element, image );
            }
            return elementImages[element];
        }



        void OptionProvider_OptionsChanged( IOption[] obj ) {
            var tracks = obj.OfType<Track>().ToArray();
            trackOptions = new HashSet<Track>(tracks);
            innateOptions = obj
                .OfType<IActionFactory>()
                .Select(x=>x.Original)
                .OfType<InnatePower>()
                .ToArray();
            this.Invalidate();
        }
        
        HashSet<Track> trackOptions;
        InnatePower[] innateOptions;

        Spirit spirit;

        public SpiritControl() {
            InitializeComponent();
            this.BackColor = Color.LightYellow;
        }

        protected override void OnPaint( PaintEventArgs pe ) {
            base.OnPaint( pe );
            if(spirit != null)
                DrawSpirit(pe.Graphics);
        }

        void DrawSpirit( Graphics graphics ) {


			using Font simpleFont = new( "Arial", 8,FontStyle.Bold ,GraphicsUnit.Point );

            using Font font = new("Arial", 18f);
            using Brush coverBrush = new SolidBrush(Color.FromArgb(128,Color.Gray));
            Brush currentBrush = Brushes.Yellow;
            using Pen highlightPen = new(Color.Red,10f); 

            const float lineHeight = 60f;
            const float deltaX = 45f;

            // Energy
            float x = 10f;
            float y = 10f;
            graphics.DrawString("Energy",simpleFont,SystemBrushes.ControlDarkDark, x, y);

            y += lineHeight;
            int revealedEnergySpaces = spirit.RevealedEnergySpaces;
            int idx = 0;
            bool highlightEnergy = trackOptions.Contains(Track.Energy);
            foreach(int en in spirit.GetEnergySequence()) {

                if(idx == revealedEnergySpaces - 1)
                    graphics.FillEllipse( currentBrush, x, y, deltaX, lineHeight );

                graphics.DrawString( en.ToString(), font, SystemBrushes.ControlDarkDark, x, y );

                if(revealedEnergySpaces <= idx)
                    graphics.FillEllipse( coverBrush, x, y, deltaX, lineHeight );

                if(highlightEnergy && revealedEnergySpaces == idx)
                    graphics.DrawEllipse( highlightPen, x, y, deltaX, lineHeight );

                x += deltaX;
                ++idx;
            }

            // Cards
            x = 10f;
            y += lineHeight;
            graphics.DrawString( "Cards", simpleFont, SystemBrushes.ControlDarkDark, x, y );

            int revealedCardSpaces = spirit.RevealedCardSpaces;
            y += lineHeight;
            idx = 0;
            bool highlightCard = trackOptions.Contains( Track.Card );
            foreach(int en in spirit.GetCardSequence()) {

                if(idx == revealedCardSpaces-1)
                    graphics.FillEllipse( currentBrush, x, y, deltaX, lineHeight );

                graphics.DrawString( en.ToString(), font, SystemBrushes.ControlDarkDark, x, y );

                if(revealedCardSpaces <= idx)
                    graphics.FillEllipse( coverBrush, x, y, deltaX, lineHeight );

                if(highlightCard && revealedCardSpaces == idx)
                    graphics.DrawEllipse( highlightPen, x, y, deltaX, lineHeight );

                x += deltaX;
                ++idx;
            }

			// Innates
			// Innates
			x = 10f;
            y += (lineHeight * 1.25f);
            foreach(var innate in spirit.InnatePowers) {
                var image = GetImage(innate);

				int drawWidth = Width - (int)x*2;
				int drawHeight = drawWidth * image.Height / image.Width;
                graphics.DrawImage(image,x,y, drawWidth, drawHeight);

				if(innateOptions.Contains( innate ))
                    graphics.DrawRectangle(highlightPen,x,y, drawWidth, drawHeight);

				y += image.Height;
                y += 10;
            }

            // activated elements
            y += 20;
            const float elementSize = 50f;
            var elements = spirit.Elements; // cache, don't recalculate

            var orderedElements = elements.Keys.OrderByDescending(el=>highestInnate[el]);
            foreach(var element in orderedElements) {
                if(elements[element] > 1) {
                    graphics.DrawString(
                        elements[element].ToString(),
                        simpleFont,
                        Brushes.Black
                        , x, y
                    );
                    x += 20;
                }
                graphics.DrawImage( GetImage( element ), x, y, elementSize, elementSize );
                x += elementSize;
                x += 10;
            }

			// !Note! - If you do not specify output width/height of image, .Net will scale image based on screen DPI and image DPI
		}

	}
}
