using System;

// forms
using System.Drawing;
using System.Windows.Forms;

namespace soundboard.Frames
{
	public class TextFrame : Form
	{
		public delegate void OKFn(string result);
		public event OKFn OnOk;

		private void CloseFn(string output = null)
		{
			if (output != null && output.Length > 0)
			{
				OnOk?.Invoke(output);
			}

			Close();
		}

		//
		// constructor
		//
		public TextFrame(string title, string content)
		{
			Text = title;
			FormBorderStyle = FormBorderStyle.FixedToolWindow;
			Size = new Size(300, 200);
			MinimumSize = Size;
			MaximumSize = Size;

			// setup elements
			Label cont = new Label();
			cont.Parent = this;
			cont.AutoSize = false;
			cont.Size = new Size(Size.Width - 25, Size.Height / 3 - 25);
			cont.Location = new Point(5, 5);
			cont.Text = content;
			cont.TextAlign = ContentAlignment.MiddleCenter;

			TextBox text = new TextBox();
			text.Parent = this;
			text.Size = new Size(Size.Width - 25, 20);
			text.Location = new Point(5, cont.Bottom + 25 * 2);

			Button cancel = new Button();
			cancel.Parent = this;
			cancel.Size = new Size(75, 20);
			cancel.Location = new Point(5, Bottom - 25 - cancel.Size.Height * 2);
			cancel.Text = "Cancel";
			cancel.Click += (object obj, EventArgs args) => CloseFn();

			Button ok = new Button();
			ok.Parent = this;
			ok.Size = new Size(75, 20);
			ok.Location = new Point(Right - 20 - ok.Size.Width, Bottom - 25 - ok.Size.Height * 2);
			ok.Text = "OK";
			ok.Click += (object obj, EventArgs args) => CloseFn(text.Text);

			// then show
			Show();
		}
	}
}
