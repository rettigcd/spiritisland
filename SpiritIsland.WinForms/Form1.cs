﻿using SpiritIsland.SinglePlayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public partial class Form1 : Form, IHaveOptions {

		public Form1() {
			InitializeComponent();
		}

		public event Action<IDecision> NewDecision;

		void Form1_Load( object sender, EventArgs e ) {

			var config = new ConfigureGame();
			if(config.ShowDialog() != DialogResult.OK) { return; }
			this.game = config.Game;

			this.islandControl.Init( game.GameState, this, config.Color );
			this.cardControl.Init( game.Spirit, this );
			this.spiritControl.Init( game.Spirit, config.Color, this );
			this.statusControl1.Init( game.GameState, this );
			this.NewDecision += UpdateButtons;

			this.islandControl.SpaceClicked += Select;
			this.islandControl.InvaderClicked += Select;
			this.cardControl.CardSelected += Select;
			this.spiritControl.OptionSelected += Select;

			ShowOptions();
		}

		void Select( IOption option ) {
			this.game.DecisionProvider.Choose( option );
			
			if(this.game.WinLoseStatus == WinLoseStatus.Playing) {
				this.ShowOptions();
				return;
			}

			this.Text = this.game.WinLoseStatus.ToString();
			// ! clear out all options

		}

		void ShowOptions() {
			IDecision decision = game.DecisionProvider.GetCurrent();
			this.promptLabel.Text = decision.Prompt;
			islandControl.Invalidate();
			NewDecision?.Invoke( decision );
		}

		#region Buttons

		void UpdateButtons( IDecision decision ) {
			ReleaseOldButtons();
			int x = CalcWidth(this.promptLabel.Text);
			for(int i = 0; i < decision.Options.Length; ++i) {
				x += AddOptionButton( decision.Options[i], x, 0 ).Width;
				x += 10;
			}
		}

		static int CalcWidth(string s ) => s.Length * 7 + 20;

		Size AddOptionButton( IOption option, int x, int y ) {
			Size sz = new Size( CalcWidth( option.Text ), 30);
			var btn = new System.Windows.Forms.Button {
				Dock = DockStyle.None,
				Location = new Point( x, y ),
				Text = option.Text,
				Size = sz,
				Tag = option
			};
			btn.Click += Btn_Click;
			this.textPanel.Controls.Add( btn );
			buttons.Add( btn );
			return sz;
		}

		void ReleaseOldButtons() {
			foreach(var old in buttons) {
				old.Click -= Btn_Click;
				this.textPanel.Controls.Remove( old );
			}
			buttons.Clear();
		}

		void Btn_Click( object sender, EventArgs e ) {
			var btn = (Button)sender;
			this.Select((IOption)btn.Tag);
		}

		#endregion

		readonly List<Button> buttons = new();
		SinglePlayerGame game;

	}

	public interface IHaveOptions {
		event Action<IDecision> NewDecision;
    }

}
