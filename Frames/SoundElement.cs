#pragma warning disable CS1690

using System;

// forms
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using static soundboard.Misc.Key;

namespace soundboard.Frames
{
	public class SoundElement : Label
	{
		// events
		public delegate void OnRemoveFn(SoundElement soundElement);
		public event OnRemoveFn OnRemove;

		//
		// const
		//
		public static int ResizeSize = 10;
		public static int BorderSize = 2;
		public static Size MinSize = new Size(ResizeSize * 2, ResizeSize * 2);

		//
		// Editing mode
		//
		public static bool IsEditing = true;
		public static bool IsResizing = false;

		// offset
		public Point offset = new Point();

		// base sound
		public SBEngine.Sound baseSound;

		// font
		public static Font CreateFont() => new Font(new FontFamily("Arial"), g.vars.FontSize);

		public static Font paintFont;

		//
		// methods
		//
		public void SetPos(int x, int y)
		{
			Location = new Point(x, y);

			baseSound.x = x;
			baseSound.y = y;
		}

		public void SetSize(int w, int h)
		{
			Size = new Size(w, h);

			baseSound.w = w;
			baseSound.h = h;
		}

		//
		// constructor
		//
		public SoundElement(SBEngine.Sound sound) : base()
		{
			//
			// set sound variable
			//
			if (!sound.IsSound())
				SBEngine.ThrowError($"Error to create SoundElement {sound} (this is not sound)");

			g.engine.soundElements.Add(this);

			baseSound = sound;

			// paint bg
			BackColor = Color.Transparent;

			Text = $"{(g.vars.hotkeySound != KeyDict.Disable ? $"[{baseSound.ID}] " : "")}{baseSound.name()}";
			Font = paintFont;
			AutoSize = false;
			ForeColor = Color.White;
			TextAlign = ContentAlignment.MiddleCenter;

			//
			// setup elements
			//
			// resize button
			var ResizeButton = new DrawElement(Size.Width - ResizeSize, Size.Height - ResizeSize, ResizeSize, ResizeSize, this, Brushes.LightBlue);
			ResizeButton.onResize = (DrawElement obj, int x, int y, int w, int h) =>
			{
				if (IsEditing)
				{
					obj.x = w - ResizeSize;
					obj.y = h - ResizeSize;
				}
			};
			ResizeButton.OnClickEvent = (DrawElement obj, MouseEventArgs args) => IsResizing = IsEditing;

			// close button
			var CloseButton = new DrawElement(0, 0, ResizeSize, ResizeSize, this, Brushes.Red);
			CloseButton.OnClickEvent = (DrawElement obj, MouseEventArgs args) =>
			{
				if (IsEditing)
					Remove();
			};

			//
			// on click
			//
			MouseDown += (object obj, MouseEventArgs args) =>
			{
				//
				// editing mode
				//
				if (IsEditing)
				{
					var self = (SoundElement)obj;

					offset.X = args.X;
					offset.Y = args.Y;
				}
				// default mode
				//
				// play sounds etc...
				else if (!IsEditing && args.Button == MouseButtons.Left)
				{
					g.engine.PlaySound(baseSound);
				}
			};

			MouseUp += (object obj, MouseEventArgs args) =>
			{
				if (args.Button == MouseButtons.Left)
					IsResizing = false;
			};

			//
			// mouse scroll
			//
			MouseWheel += (object obj, MouseEventArgs args) =>
			{
				// play sound
				if (args.Delta != 0)
					g.engine.PlaySound(baseSound);
			};

			//
			// move element
			//
			MouseMove += (object obj, MouseEventArgs args) =>
			{
				if (IsEditing && args.Button == MouseButtons.Left)
				{
					var self = (SoundElement)obj;
					Frame frame = (Frame)((Panel)self.Parent).Parent;

					if (IsResizing)
					{
						var newSize = self.Size;

						newSize.Width = Math.Max(args.X + 5, MinSize.Width);
						newSize.Height = Math.Max(args.Y + 5, MinSize.Height);

						// snap
						if (frame.GridEnable)
						{
							newSize.Width = (int)(Math.Round(newSize.Width / (double)g.vars.GridSize) * g.vars.GridSize);
							newSize.Height = (int)(Math.Round(newSize.Height / (double)g.vars.GridSize) * g.vars.GridSize);
						}

						SetSize(newSize.Width, newSize.Height);

						self.Invalidate();
					}
					else
					{
						var newLoc = self.Location;

						newLoc.X += args.X - offset.X;
						newLoc.Y += args.Y - offset.Y;

						// snap
						if (frame.GridEnable)
						{
							newLoc.X = (int)(Math.Round(newLoc.X / (double)g.vars.GridSize) * g.vars.GridSize); // 4 - snap size
							newLoc.Y = (int)(Math.Round(newLoc.Y / (double)g.vars.GridSize) * g.vars.GridSize);
						}

						SetPos(Math.Max(newLoc.X, 0), Math.Max(newLoc.Y, 0));
					}
				}
			};

			Resize += (object obj, EventArgs args) =>
			{
				if (((Frame)((Panel)((SoundElement)obj).Parent).Parent).GridEnable)
					g.MainFrame.RepaintGlobal();
			};

			//
			// paint
			//
			Paint += (object obj, PaintEventArgs args) =>
			{
				var graphic = args.Graphics;
				var self = (SoundElement)obj;

				// draw border
				if (IsEditing)
				{
					graphic.DrawRectangle(new Pen(Color.Gray, BorderSize), new Rectangle(1, 1, self.Size.Width - 2, self.Size.Height - 2));
				}
				else
				{
					Pen pen = Pens.Cyan;

					if (g.engine.currentSound == baseSound && !g.engine.isOnDelay)
						pen = Pens.Blue;
					else if (g.engine.currentSound == baseSound && g.engine.isOnDelay)
						pen = Pens.DarkGreen;

					graphic.DrawRectangle(pen, new Rectangle(0, 0, self.Size.Width - 1, self.Size.Height - 1));
				}
			};

			g.engine.OnTrackStart += (SBEngine.Sound soundStarted, bool isDelay) => RepaintAllElements();
			g.engine.OnTrackFinish += (SBEngine.Sound soundFinished) => RepaintAllElements();
		}

		//
		// Remove method
		//
		public void Remove()
		{
			if (g.engine.currentSound == baseSound)
				g.engine.StopSound(true);

			OnRemove?.Invoke(this);

			Controls.Remove(this);
			Dispose();
		}

		// retext method
		public static void RetextAllElements()
		{
			if (g.engine.soundElements.Count() == 0)
				return;

			foreach (var el in g.engine.soundElements)
			{
				el.Text = $"{(g.vars.hotkeySound != KeyDict.Disable ? $"[{el.baseSound.ID}] " : "")}{el.baseSound.name()}";
			}

			RepaintAllElements();
		}

		// refont method
		public static void RefontAllElements()
		{
			if (g.engine.soundElements.Count() == 0)
				return;

			paintFont = CreateFont();

			foreach (var el in g.engine.soundElements)
			{
				el.Font = paintFont;
			}

			RepaintAllElements();
		}

		// repaint method
		public static void RepaintAllElements()
		{
			if (g.engine.soundElements.Count() == 0)
				return;

			foreach (var element in g.engine.soundElements)
			{
				element.Invalidate();
			}
		}

		//
		// set edit mode
		//
		public static void SetEditMode(bool mode)
		{
			IsEditing = mode;

			RepaintAllElements();
		}
	}
}
