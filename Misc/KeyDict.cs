using System;

using System.Collections.Generic;

// forms
using System.Drawing;
using System.Windows.Forms;

using static soundboard.Misc.WinAPI;

namespace soundboard.Misc
{
	public static class Key
	{
		public enum KeyDict
		{
			Disable = 0,
			Ctrl,
			Alt,
			CtrlAlt
		}

		public static KeyDict BoolToKeyDict(bool isCtrl, bool isAlt) => isCtrl && isAlt ? KeyDict.CtrlAlt : (isCtrl && !isAlt ? KeyDict.Ctrl : (!isCtrl && isAlt ? KeyDict.Alt : KeyDict.Disable));

		public static Dictionary<KeyDict, string> soundDelayKeyDict = new Dictionary<KeyDict, string>()
		{
			{ KeyDict.Ctrl, "Ctrl" },
			{ KeyDict.Alt, "Alt" },

			{ KeyDict.CtrlAlt, "Ctrl + Alt" },
		};

		public static bool IsPressing(KeyDict key)
		{
			bool pressing = true;
			bool IsAlt = key == KeyDict.Alt || key == KeyDict.CtrlAlt;
			bool IsCtrl = key == KeyDict.Ctrl || key == KeyDict.CtrlAlt;

			if (IsAlt && !GetAsyncKeyState(VK.ALT))
				pressing = false;
			if (IsCtrl && !GetAsyncKeyState(VK.CONTROL))
				pressing = false;

			return pressing;
		}
	}

	//
	// keydictpanel
	//
	public class KeyDictPanel : GroupBox
	{
		public delegate void OnBindChangedFn(object obj, bool isCtrl, bool isAlt);
		public event OnBindChangedFn BindChanged;

		public CheckBox alt = new CheckBox();
		public CheckBox ctrl = new CheckBox();

		public void Update(Key.KeyDict key)
		{
			alt.Checked = key == Key.KeyDict.Alt || key == Key.KeyDict.CtrlAlt;
			ctrl.Checked = key == Key.KeyDict.Ctrl || key == Key.KeyDict.CtrlAlt;
		}

		public int DefaultHeight { get; } = 45;

		public void CallEvent() => BindChanged?.Invoke(this, ctrl.Checked, alt.Checked);

		public KeyDictPanel(string desctiption)
		{
			// description
			Text = desctiption;
			Height = DefaultHeight;

			// CTRL
			ctrl.Parent = this;
			ctrl.AutoSize = true;
			ctrl.Location = new Point(alt.Right, Size.Height / 2 - ctrl.Size.Height / 3);
			ctrl.Text = "Ctrl";
			ctrl.CheckedChanged += (object obj, EventArgs args) => CallEvent();

			// ALT
			alt.Parent = this;
			alt.AutoSize = true;
			alt.Location = new Point(0, Size.Height / 2 - alt.Size.Height / 3);
			alt.Text = "Alt";
			alt.CheckedChanged += (object obj, EventArgs args) => CallEvent();
		}
	}
}
