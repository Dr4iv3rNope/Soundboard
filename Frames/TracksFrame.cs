#pragma warning disable CS1690

using System;

// Froms
using System.Drawing;
using System.Windows.Forms;

namespace soundboard.Frames
{
	public class TracksFrame : Dr4iv3rForm
	{
		public ListView list;

		public void AddSound(SoundElement el)
		{
			el.OnRemove += (SoundElement element) => RemoveSound(element.baseSound);

			var item = new ListViewItem(new string[] { el.baseSound.ID.ToString(), el.baseSound.name() });

			list.Items.Add(item);
		}

		public void RemoveSound(int uID)
		{
			for(var item = 0; item < list.Items.Count; item++)
			{
				if (list.Items[item].SubItems[0].Text == uID.ToString())
					list.Items[item].Remove();
			}
		}
		public void RemoveSound(SBEngine.Sound sound) => RemoveSound(sound.ID);

		public void Reload()
		{
			list.Items.Clear();

			foreach (var el in g.engine.soundElements)
				AddSound(el);
		}

		public ListViewItem GetCurrentItem() => list.SelectedItems[0];
		public int GetCurrentID() => Convert.ToInt32(GetCurrentItem().SubItems[0].Text);
		public string GetCurrentName() => GetCurrentItem().SubItems[1].Text;

		public void Repaint(bool isDelay = false)
		{
			if (!list.InvokeRequired)
				return;

			var cur = g.engine.currentSound.ID.ToString();

			list.BeginInvoke((Action)delegate()
			{
				foreach (ListViewItem item in list.Items)
				{
					if (cur == item.SubItems[0].Text)
					{
						item.BackColor = isDelay ? Color.Green : Color.LightBlue;
						item.ForeColor = Color.Black;
					}
					else
					{
						item.BackColor = Color.White;
						item.ForeColor = Color.Black;
					}
				}
			});
		}

		public TracksFrame()
		{
			// events
			g.engine.OnTrackStart += (SBEngine.Sound sound, bool isDelay) => Repaint(isDelay);
			g.engine.OnTrackFinish += (SBEngine.Sound sound) => Repaint();
			g.engine.OnTrackAdd += (SoundElement sound) => AddSound(sound);

			g.MainFrame.FormClosed += (object obj, FormClosedEventArgs args) => Close();

			list = new ListView();

			// setup window
			Text = $"{SBEngine.PName} Track List";
			MinimumSize = new Size(300, 100);

			KeyDown += (object obj, KeyEventArgs args) =>
			{
				if(args.KeyCode == Keys.Delete)
				{
					g.engine.RemoveSound(GetCurrentID());
				}
			};

			// list
			list.Parent = this;
			list.View = View.Details;

			list.GridLines = true;
			list.FullRowSelect = true;
			list.MultiSelect = false;

			list.Dock = DockStyle.Fill;
			list.Columns.Add("ID", 50);
			list.Columns.Add("Name", 300);

			list.DoubleClick += (object obj, EventArgs args) => g.engine.PlaySound(Convert.ToInt32(list.SelectedItems[0].SubItems[0].Text));
		}
	}
}
