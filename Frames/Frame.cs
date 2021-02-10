using System;

using System.Threading;

// forms
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace soundboard.Frames
{
	//
	// Window frame
	//
	public class Frame : Form
	{
		public bool SelectItems = false;
		public Point SelectItemsPos = new Point();
		public Point SelectItemsCur = new Point();

		// global panel
		public Panel global;

		// menu bar
		public MenuStrip menubar;

		// label info
		public Label info;
		// timer
		public Thread timer;

		// update elements
		public void UpdateElements()
		{
			info.Visible = g.vars.EnableInfo;
			global.AutoScroll = g.vars.EnableScrolling;

			RepaintGlobal();
		}

		//
		// repaint global
		//
		public void RepaintGlobal() => global.Invalidate();

		// grid
		public bool GridEnable = false;

		//
		// contructor
		//
		public Frame()
		{
			Text = SBEngine.PName;
			AllowDrop = true;

			MinimumSize = new Size(300, 300);

			// events
			DragEnter += new DragEventHandler(Hook.DragEnter);
			DragDrop += new DragEventHandler(Hook.DragDrop);
			KeyDown += new KeyEventHandler(Hook.KeyPress);
			KeyUp += new KeyEventHandler(Hook.KeyUp);
			MouseHover += (object obj, EventArgs args) =>
			{
				Cursor.Current = Cursors.Cross;
				Application.DoEvents();
			};
			MouseLeave += (object obj, EventArgs args) =>
			{
				Cursor.Current = Cursors.Default;
				Application.DoEvents();
			};
			Resize += (object obj, EventArgs args) =>
			{
				RepaintGlobal();
			};
			FormClosing += (object obj, FormClosingEventArgs args) => g.ProgramWorking = false;

			// init timer for info Label
			timer = new Thread(() =>
		   {
			   while (g.ProgramWorking)
			   {
				   Thread.Sleep(50);

				   if (info == null || !g.vars.EnableInfo || !info.InvokeRequired)
					   continue;

				   try
				   {
					   info?.Invoke((Action)delegate ()
					   {
						   info.Text = $"Sound count: {g.engine.soundElements.Count()} | " +
							   $"Current sound: {g.engine.currentSound.name()} | " +
							   $"Mode: {(SoundElement.IsEditing ? "Edit" : "Normal")}";
					   });
				   }
				   catch { }
			   }
		   });
			timer.Start();

			//
			// init global window
			//
			global = new Panel();
			global.Parent = this;
			global.Dock = DockStyle.Fill;
			global.AutoScroll = true;

			global.MouseDown += new MouseEventHandler(Hook.MouseDown);
			global.Paint += new PaintEventHandler(Hook.Paint);

			global.BackColor = Color.Black; // bg

			//
			// init info panel
			//
			info = new Label();
			info.Parent = this;

			info.Size = new Size(0, 14);

			info.Dock = DockStyle.Bottom;

			info.BackColor = Color.Gray;
			info.ForeColor = Color.White;

			info.Visible = g.vars.EnableInfo;

			//
			// init menu bar
			//
			menubar = new MenuStrip();
			menubar.Parent = this;
			menubar.Dock = DockStyle.Top;

			// Bar -> Debug -> ...;
			ToolStripMenuItem reloadfont = new ToolStripMenuItem("Reload font", null, (object obj, EventArgs args) => SoundElement.RefontAllElements());
			ToolStripMenuItem repaint = new ToolStripMenuItem("Repaint all", null, (object obj, EventArgs args) => SoundElement.RepaintAllElements());
			ToolStripMenuItem reloadtext = new ToolStripMenuItem("Reload text", null, (object obj, EventArgs args) => SoundElement.RetextAllElements());
			ToolStripMenuItem reloadFrame = new ToolStripMenuItem("Force update main frame", null, (object obj, EventArgs args) => UpdateElements());
			ToolStripMenuItem reloadDevices = new ToolStripMenuItem("Reload devices", null, (object obj, EventArgs args) => WindowSettings.DeviceButton.ReloadDeviceButtons());
			ToolStripMenuItem reloadOptions = new ToolStripMenuItem("Reload options frame", null, (object obj, EventArgs args) => WindowSettings.GUI.SetupAllValues());
			ToolStripMenuItem stopsound = new ToolStripMenuItem("Force stop sound", null, (object obj, EventArgs args) => g.engine.StopSound(true));


			ToolStripMenuItem fileSaveOld = new ToolStripMenuItem("Save sounds (Old method)", null, (object obj, EventArgs args) => g.engine.config.SaveOld());
			ToolStripMenuItem fileLoadOld = new ToolStripMenuItem("Load sounds (Old method)", null, (object obj, EventArgs args) => g.engine.config.LoadOld());

			//
			// Bar -> Debug -> Old features
			//
			ToolStripMenuItem oldfeat = new ToolStripMenuItem("Old Features");
			oldfeat.DropDownItems.AddRange(new ToolStripItem[] { fileSaveOld, fileLoadOld });

			//
			// Bar -> Debug
			//
			ToolStripMenuItem debug = new ToolStripMenuItem("Debug");
			debug.DropDownItems.AddRange(new ToolStripItem[] { reloadfont, repaint, reloadtext, reloadFrame, reloadDevices, reloadOptions, stopsound, oldfeat });



			// Bar -> Options -> Settings;
			ToolStripMenuItem settings = new ToolStripMenuItem("Settings", null, (object obj, EventArgs args) => g.Settings.Show());

			//
			// Bar -> Options
			//
			ToolStripMenuItem options = new ToolStripMenuItem("Options");
			options.DropDownItems.AddRange(new ToolStripItem[] { settings });



			// Bar -> View -> Open track window;
			ToolStripMenuItem opentrack = new ToolStripMenuItem("Open Tracks Window", null, (object obj, EventArgs args) => g.tracksFrame.Show());

			//
			// Bar -> View
			//
			ToolStripMenuItem View = new ToolStripMenuItem("View");
			View.DropDownItems.AddRange(new ToolStripItem[] { opentrack });



			// Bar -> Edit -> Set edit mode;
			ToolStripMenuItem editmodeOn = new ToolStripMenuItem("Set edit mode", null, (object obj, EventArgs args) => SoundElement.SetEditMode(true));

			// Bar -> Edit -> Set normal mode;
			ToolStripMenuItem editmodeOff = new ToolStripMenuItem("Set normal mode", null, (object obj, EventArgs args) => SoundElement.SetEditMode(false));

			//
			// Bar -> Edit -> Mode
			//
			ToolStripMenuItem editmode = new ToolStripMenuItem("[TAB | MMB] Mode");
			editmode.DropDownItems.AddRange(new ToolStripItem[] { editmodeOn, editmodeOff });

			// Bar -> Edit -> Clear all elements;
			ToolStripMenuItem editClear = new ToolStripMenuItem("Clear all elements", null, (object obj, EventArgs args) =>
			{
				if (MessageBox.Show("Are you sure?", "what.", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					g.engine.ClearSoundElements();
			});

			//
			// Bar -> Edit
			//
			ToolStripMenuItem edit = new ToolStripMenuItem("Edit");
			edit.DropDownItems.AddRange(new ToolStripItem[] { editmode, editClear });




			// Bar -> File -> Save;
			ToolStripMenuItem fileSave = new ToolStripMenuItem("[Ctrl + S] Save sounds", null, (object obj, EventArgs args) => g.engine.config.SaveSound());
			// Bar -> File -> Load;
			ToolStripMenuItem fileLoad = new ToolStripMenuItem("[Ctrl + O] Load sounds", null, (object obj, EventArgs args) => g.engine.config.LoadSound());

			ToolStripMenuItem fileAdd = new ToolStripMenuItem("[Ctrl + A] Add sound", null, (object obj, EventArgs args) => g.engine.config.AddSoundFromFileBrowser());



			//
			// Bar -> File -> Save/Load ;
			//
			ToolStripMenuItem saveload = new ToolStripMenuItem("Save/Load");
			// add elements to
			// Bar -> Options -> ...
			saveload.DropDownItems.AddRange(new ToolStripItem[] { fileSave, fileLoad, fileAdd });

			//
			// Bar -> File -> ... ;
			//
			ToolStripMenuItem file = new ToolStripMenuItem("File");
			// add elements to
			// Bar -> Options -> ...
			file.DropDownItems.AddRange(new ToolStripItem[] { saveload });



			//
			// add elements to
			// Bar -> ...
			//
			menubar.Items.AddRange(new ToolStripItem[] { file, edit, View, options, debug });
		}

		//
		// Hooks
		//
		public class Hook
		{
			public static void DragDrop(object obj, DragEventArgs args)
			{
				Console.WriteLine("Hook.DragDrop called");
				var files = (string[])args.Data.GetData(DataFormats.FileDrop);

				foreach (string path in files)
				{
					var self = (Frame)obj;

					// filter path
					bool allow = path.EndsWith(".mp3") || path.EndsWith(".wav");

					if (allow)
						//g.engine.AddSound(path, (int)((float)args.X / 1.1) - ((Frame)obj).Location.X, (int)((float)args.Y / 1.1) - ((Frame)obj).Location.Y);
						g.engine.AddSound(path, args.X - self.Location.X - self.global.Location.X, args.Y - self.Location.Y - self.global.Location.Y);
				}
			}

			public static void DragEnter(object obj, DragEventArgs args)
			{
				args.Effect = DragDropEffects.All;
			}

			public static void MouseDown(object obj, MouseEventArgs args)
			{
				if (args.Button == MouseButtons.Middle)
				{
					SoundElement.SetEditMode(!SoundElement.IsEditing);
				}
				else if (!SoundElement.IsEditing && args.Button == MouseButtons.Right)
				{
					g.engine.StopSound(true);
				}
			}

			public static void KeyPress(object obj, KeyEventArgs args)
			{
				var self = (Frame)obj;

				if (args.Shift && !self.GridEnable)
				{
					self.GridEnable = true;
					self.RepaintGlobal();
				}

				if (args.KeyCode == Keys.Tab)
					SoundElement.SetEditMode(!SoundElement.IsEditing);

				if (args.Control && args.KeyCode == Keys.S)
					g.engine.config.SaveSound();

				if (args.Control && args.KeyCode == Keys.O)
					g.engine.config.LoadSound();

				if (args.Control && args.KeyCode == Keys.A)
					g.engine.config.AddSoundFromFileBrowser();
			}

			public static void KeyUp(object obj, KeyEventArgs args)
			{
				var self = (Frame)obj;

				if (args.KeyCode == Keys.ShiftKey)
				{
					self.GridEnable = false;
					self.RepaintGlobal();
				}
			}

			private static Pen paintPen = new Pen(Color.FromArgb(40, 40, 40), 1f);

			public static void Paint(object obj, PaintEventArgs args)
			{
				Frame parent = (Frame)((Panel)obj).Parent;

				Console.WriteLine($"Grid: {parent.GridEnable}");

				if (parent.GridEnable)
				{
					var self = (Panel)obj;
					var graphic = args.Graphics;

					// draw grid
					for (int x = 0; x + g.vars.GridSize <= self.Height + g.vars.GridSize; x += g.vars.GridSize)
					{
						graphic.DrawLine(paintPen, new Point(0, x), new Point(self.Width, x));
					}

					for (int y = 0; y + g.vars.GridSize <= self.Width + g.vars.GridSize; y += g.vars.GridSize)
					{
						graphic.DrawLine(paintPen, new Point(y, 0), new Point(y, self.Height));
					}
				}
			}
		}
	}
}
