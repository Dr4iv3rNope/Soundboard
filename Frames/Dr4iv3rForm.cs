using System;

// forms
using System.Drawing;
using System.Windows.Forms;

namespace soundboard.Frames
{
	public class Dr4iv3rForm : Form
	{
		protected override void OnFormClosing(FormClosingEventArgs args)
		{
			base.OnFormClosing(args);

			Console.WriteLine($"Close reason: {args.CloseReason}");

			if (args.CloseReason == CloseReason.UserClosing)
				args.Cancel = true;

			Hide();
		}
	}
}
