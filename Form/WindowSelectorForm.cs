using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TianXiaMiner.Actions;

namespace TianXiaMiner
{
    public partial class WindowSelectorForm : Form
    {
        private List<FindAllWindowsAction.GameWindowInfo> _allWindows;
        private List<FindAllWindowsAction.GameWindowInfo> _selectedWindows;

        public List<FindAllWindowsAction.GameWindowInfo> SelectedWindows => _selectedWindows;

        public WindowSelectorForm(List<FindAllWindowsAction.GameWindowInfo> windows)
        {
            InitializeComponent();
            _allWindows = windows;
            _selectedWindows = new List<FindAllWindowsAction.GameWindowInfo>();

            LoadWindows();
            UpdateCountLabel();
            BindEvents();
        }

        private void BindEvents()
        {
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            btnSelectAll.Click += BtnSelectAll_Click;
            btnSelectNone.Click += BtnSelectNone_Click;
            clbWindows.ItemCheck += ClbWindows_ItemCheck;
        }

        private void LoadWindows()
        {
            lblTip.Text = $"找到 {_allWindows.Count} 个游戏窗口，请勾选要采集的窗口：";

            clbWindows.Items.Clear();
            for (int i = 0; i < _allWindows.Count; i++)
            {
                var w = _allWindows[i];
                string displayText = $"{i + 1}. {w.Title} (位置: {w.X},{w.Y}) 大小: {w.Width}x{w.Height}";
                clbWindows.Items.Add(displayText, true);
            }
        }

        private void UpdateCountLabel()
        {
            int selectedCount = 0;
            for (int i = 0; i < clbWindows.Items.Count; i++)
            {
                if (clbWindows.GetItemChecked(i))
                    selectedCount++;
            }
            lblCount.Text = $"已选择 {selectedCount} / {clbWindows.Items.Count} 个窗口";
            lblCount.ForeColor = selectedCount > 0 ? Color.FromArgb(46, 139, 87) : Color.Red;
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbWindows.Items.Count; i++)
                clbWindows.SetItemChecked(i, true);
            UpdateCountLabel();
        }

        private void BtnSelectNone_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbWindows.Items.Count; i++)
                clbWindows.SetItemChecked(i, false);
            UpdateCountLabel();
        }

        private void ClbWindows_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((Action)(() => UpdateCountLabel()));
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            _selectedWindows.Clear();
            for (int i = 0; i < clbWindows.Items.Count; i++)
            {
                if (clbWindows.GetItemChecked(i))
                {
                    _selectedWindows.Add(_allWindows[i]);
                }
            }

            if (_selectedWindows.Count == 0)
            {
                MessageBox.Show("请至少选择一个窗口！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}