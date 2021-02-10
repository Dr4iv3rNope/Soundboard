using System;

using System.Collections.Generic;

// forms
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace soundboard.Frames
{
	public class DrawElement
	{
		public delegate void OnResize(DrawElement obj, int x, int y, int w, int h);
		public OnResize onResize = null;

		public delegate void Click(DrawElement obj, MouseEventArgs args);
		public Click OnClickEvent = null;

		public static List<DrawElement> DrawElements = new List<DrawElement>();

		public int x, y;
		public int w, h;

		// parent
		public SoundElement parent;

		// paint
		public Brush paintColor;
		public bool editOnly;

		public DrawElement(int x, int y, int w, int h, SoundElement element, Brush paintColor, bool editOnly = true)
		{
			this.x = x;
			this.y = y;
			this.w = w;
			this.h = h;

			this.paintColor = paintColor;
			this.editOnly = editOnly;

			parent = element;

			DrawElements.Add(this);

			InitEvents();
		}

		public void InitEvents()
		{
			parent.Resize += (object obj, EventArgs args) => onResize?.Invoke(this, parent.Location.X, parent.Location.Y, parent.Size.Width, parent.Size.Height);

			parent.MouseDown += (object obj, MouseEventArgs args) =>
			{
				if (args.Button == MouseButtons.Left &&
					Enumerable.Range(x, x + w).Contains(args.X) &&
					Enumerable.Range(y, y + h).Contains(args.Y))

					OnClickEvent?.Invoke(this, args);
			};

			parent.Paint += (object obj, PaintEventArgs args) =>
			{
				if (!SoundElement.IsEditing && this.editOnly)
					return;

				var graphic = args.Graphics;

				graphic.FillRectangle(this.paintColor, new Rectangle(x, y, w, h));
			};
		}
	}
}
