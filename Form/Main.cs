using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianXiaMiner.Core;
using TianXiaMiner.Actions;
using TianXiaMiner.Utils;

namespace TianXiaMiner
{
    public partial class Main : Form
    {
        private ImageRecognition _imageRec;
        private HumanSimulator _human;
        private WindowHelper _window;
        private GameDelay _delay;
        private FindAllWindowsAction _findWindowsAction;
        private List<FindAllWindowsAction.GameWindowInfo> _foundWindows;

        // 自动运行控制变量
        private bool _isAutoRunning = false;
        private bool _isPaused = false;
        private CancellationTokenSource _cancellationTokenSource;

        public Main()
        {
            InitializeComponent();

            // 初始化核心类
            _imageRec = new ImageRecognition();
            _human = new HumanSimulator();
            _window = new WindowHelper();
            _delay = new GameDelay();

            // 初始化找窗口动作
            _findWindowsAction = new FindAllWindowsAction(_imageRec, _human, _window, _delay);
            _foundWindows = new List<FindAllWindowsAction.GameWindowInfo>();

            // 初始化取消令牌
            _cancellationTokenSource = new CancellationTokenSource();

            // 绑定 ListView 的鼠标点击事件（用于右键复制）
            listLog.MouseClick += ListLog_MouseClick;
            // 绑定键盘事件（支持 Ctrl+C）
            listLog.KeyDown += ListLog_KeyDown;

            this.Load += Form1_Load;
        }

        /// <summary>
        /// ListView 鼠标点击事件 - 支持右键复制
        /// </summary>
        private void ListLog_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // 获取点击位置的项目
                ListViewItem item = listLog.GetItemAt(e.X, e.Y);
                if (item != null)
                {
                    // 选中点击的项目
                    item.Selected = true;

                    // 创建右键菜单
                    ContextMenuStrip contextMenu = new ContextMenuStrip();

                    // 复制选中项
                    ToolStripMenuItem copySelectedItem = new ToolStripMenuItem("复制选中");
                    copySelectedItem.Click += (s, ev) => CopySelectedLog();
                    contextMenu.Items.Add(copySelectedItem);

                    // 复制所有日志
                    ToolStripMenuItem copyAllItem = new ToolStripMenuItem("复制所有日志");
                    copyAllItem.Click += (s, ev) => CopyAllLog();
                    contextMenu.Items.Add(copyAllItem);

                    // 分隔线
                    contextMenu.Items.Add(new ToolStripSeparator());

                    // 清空日志
                    ToolStripMenuItem clearItem = new ToolStripMenuItem("清空日志");
                    clearItem.Click += (s, ev) => LogHelper.Clear();
                    contextMenu.Items.Add(clearItem);

                    contextMenu.Show(listLog, e.Location);
                }
                else
                {
                    // 点击空白区域，只显示复制所有和清空
                    ContextMenuStrip contextMenu = new ContextMenuStrip();

                    ToolStripMenuItem copyAllItem = new ToolStripMenuItem("复制所有日志");
                    copyAllItem.Click += (s, ev) => CopyAllLog();
                    contextMenu.Items.Add(copyAllItem);

                    contextMenu.Items.Add(new ToolStripSeparator());

                    ToolStripMenuItem clearItem = new ToolStripMenuItem("清空日志");
                    clearItem.Click += (s, ev) => LogHelper.Clear();
                    contextMenu.Items.Add(clearItem);

                    contextMenu.Show(listLog, e.Location);
                }
            }
        }

        /// <summary>
        /// ListView 键盘事件 - 支持 Ctrl+C 复制
        /// </summary>
        private void ListLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedLog();
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// 复制选中的日志
        /// </summary>
        private void CopySelectedLog()
        {
            if (listLog.SelectedItems.Count == 0)
            {
                // 如果没有选中项，提示用户
                LogHelper.LogWarning("请先选中要复制的日志（单击选中）");
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem item in listLog.SelectedItems)
            {
                // 获取时间和消息
                string time = item.SubItems[0].Text;
                string message = item.SubItems[1].Text;
                sb.AppendLine($"{time} {message}");
            }

            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
                LogHelper.LogSuccess($"已复制 {listLog.SelectedItems.Count} 条日志到剪贴板");
            }
        }

        /// <summary>
        /// 复制所有日志（从新到旧排序）
        /// </summary>
        private void CopyAllLog()
        {
            if (listLog.Items.Count == 0)
            {
                LogHelper.LogWarning("没有日志可复制");
                return;
            }

            StringBuilder sb = new StringBuilder();

            // 按顺序从旧到新复制（注意 listLog 是倒序插入的，最新在顶部）
            for (int i = listLog.Items.Count - 1; i >= 0; i--)
            {
                ListViewItem item = listLog.Items[i];
                string time = item.SubItems[0].Text;
                string message = item.SubItems[1].Text;
                sb.AppendLine($"{time} {message}");
            }

            Clipboard.SetText(sb.ToString());
            LogHelper.LogSuccess($"已复制全部 {listLog.Items.Count} 条日志到剪贴板");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 初始化日志控件
            LogHelper.Initialize(listLog);

            LogHelper.Log("=== 程序启动，自动查找窗口 ===", Color.Blue);

            // 创建需要的文件夹
            string[] folders = new string[]
            {
                Path.Combine(Application.StartupPath, "Images", "Templates"),
                Path.Combine(Application.StartupPath, "Images", "Screenshots")
            };

            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    LogHelper.Log($"创建文件夹: {folder}", Color.Gray);
                }
            }

            // 执行查找所有窗口
            bool found = _findWindowsAction.Execute();

            if (found)
            {
                // 获取所有找到的窗口
                var allWindows = _findWindowsAction.GetFoundWindows();
                LogHelper.Log($"找到 {allWindows.Count} 个游戏窗口", Color.Green);

                // 显示窗口选择对话框，让用户勾选要采集的窗口
                using (var selector = new WindowSelectorForm(allWindows))
                {
                    var result = selector.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        // 使用用户选择的窗口
                        _foundWindows = selector.SelectedWindows;
                        LogHelper.Log($"已选择 {_foundWindows.Count} 个窗口进行采集：", Color.Green);
                        for (int i = 0; i < _foundWindows.Count; i++)
                        {
                            var w = _foundWindows[i];
                            LogHelper.Log($"  {i + 1}. {w.Title} 位置({w.X},{w.Y}) 大小({w.Width}x{w.Height})", Color.Gray);
                        }

                        // 默认激活第一个选中的窗口
                        if (_foundWindows.Count > 0)
                        {
                            _findWindowsAction.ActivateWindow(_foundWindows[0]);
                            LogHelper.Log($"已激活窗口: {_foundWindows[0].Title}", Color.Green);
                        }
                    }
                    else
                    {
                        // 用户取消了选择，使用全部窗口
                        LogHelper.LogWarning("用户取消了窗口选择，将使用全部窗口");
                        _foundWindows = allWindows;
                    }
                }
            }
            else
            {
                LogHelper.LogWarning("未找到游戏窗口，请手动打开游戏");
                _foundWindows = new List<FindAllWindowsAction.GameWindowInfo>();
            }

            LogHelper.Log("=== 初始化完成 ===", Color.Blue);

            // 初始化按钮状态
            btnPause.Enabled = false;
            btnStop.Enabled = false;
        }

        /// <summary>
        /// 获取当前激活的窗口
        /// </summary>
        private FindAllWindowsAction.GameWindowInfo GetCurrentActiveWindow(FindAllWindowsAction.GameWindowInfo targetWindow = null)
        {
            if (targetWindow != null)
            {
                return targetWindow;
            }

            // 查找激活的窗口
            foreach (var w in _foundWindows)
            {
                if (w.IsActive)
                {
                    return w;
                }
            }

            // 如果没有激活的窗口，默认返回第一个
            if (_foundWindows.Count > 0)
            {
                return _foundWindows[0];
            }

            return null;
        }

        private void btnStep1_CheckItem_Click(object sender, EventArgs e)
        {
            // 获取当前激活的窗口
            var currentWindow = GetCurrentActiveWindow();
            if (currentWindow == null)
            {
                LogHelper.LogError("没有激活的窗口");
                return;
            }

            // 激活窗口
            _findWindowsAction.ActivateWindow(currentWindow);
            Thread.Sleep(500);

            // 创建动作并执行
            CheckItemAction action = new CheckItemAction(_imageRec, _human, _window, _delay);
            action.SetWindowPosition(currentWindow.X, currentWindow.Y, currentWindow.Width, currentWindow.Height);

            // 订阅日志
            action.OnLog += (logMessage) => LogHelper.Log(logMessage);

            // 执行
            bool result = action.Execute();
            if (result)
                LogHelper.LogSuccess("检测足通成功");
            else
                LogHelper.LogError("检测足通失败");
        }

        private void btnStep2_UseMaterial_Click(object sender, EventArgs e)
        {
            // 获取当前激活的窗口
            var currentWindow = GetCurrentActiveWindow();
            if (currentWindow == null)
            {
                LogHelper.LogError("没有激活的窗口");
                return;
            }

            // 激活窗口
            LogHelper.Log($"正在激活窗口: {currentWindow.Title}");
            _findWindowsAction.ActivateWindow(currentWindow);
            Thread.Sleep(500);

            // 刷新窗口坐标
            RefreshWindowPosition(currentWindow);

            // 创建动作并执行
            UseMaterialAction action = new UseMaterialAction(_imageRec, _human, _window, _delay);

            // 订阅日志事件，把Action里的日志显示到界面上
            action.OnLog += (logMessage) => LogHelper.Log(logMessage);

            LogHelper.Log($"准备执行动作2，窗口: {currentWindow.Title} 位置({currentWindow.X},{currentWindow.Y})");

            // 动作内部自己处理所有配置和日志
            bool result = action.ExecuteWithWindow(currentWindow);

            if (result)
                LogHelper.LogSuccess("动作2执行成功");
            else
                LogHelper.LogError("动作2执行失败");
        }

        /// <summary>
        /// 刷新窗口坐标
        /// </summary>
        private void RefreshWindowPosition(FindAllWindowsAction.GameWindowInfo window)
        {
            var rect = new WindowHelper.RECT();
            if (WindowHelper.GetWindowRect(window.Hwnd, ref rect))
            {
                window.X = rect.Left;
                window.Y = rect.Top;
                window.Width = rect.Right - rect.Left;
                window.Height = rect.Bottom - rect.Top;
            }
        }

        private void btnStep3_FindOnMap_Click(object sender, EventArgs e)
        {
            LogHelper.Log("=== 动作3: 地图找资源 ===", Color.Blue);

            // 获取当前激活的窗口
            var currentWindow = GetCurrentActiveWindow();
            if (currentWindow == null)
            {
                LogHelper.LogError("没有激活的窗口");
                return;
            }

            // 激活窗口
            LogHelper.Log($"正在激活窗口: {currentWindow.Title}");
            _findWindowsAction.ActivateWindow(currentWindow);
            Thread.Sleep(500);

            // 刷新窗口坐标
            RefreshWindowPosition(currentWindow);

            // 创建动作3
            UseMapAction action = new UseMapAction(_imageRec, _human, _window, _delay);

            // 订阅日志
            action.OnLog += (logMessage) => LogHelper.Log(logMessage);

            // 执行动作
            bool result = action.Execute(currentWindow);

            if (result)
                LogHelper.LogSuccess("动作3执行成功");
            else
                LogHelper.LogError("动作3执行失败");
        }

        private void btnStep4_CollectResource_Click(object sender, EventArgs e)
        {
            var currentWindow = GetCurrentActiveWindow();
            if (currentWindow == null)
            {
                LogHelper.LogError("没有激活的窗口");
                return;
            }

            LogHelper.Log($"正在激活窗口: {currentWindow.Title}");
            _findWindowsAction.ActivateWindow(currentWindow);
            Thread.Sleep(500);

            RefreshWindowPosition(currentWindow);

            CollectResourceAction action = new CollectResourceAction(_imageRec, _human, _window, _delay);
            action.OnLog += (logMessage) => LogHelper.Log(logMessage);

            bool result = action.ExecuteWithWindow(currentWindow);
            if (result)
                LogHelper.LogSuccess("动作4执行成功");
            else
                LogHelper.LogError("动作4执行失败");
        }

        private void btnStep5_GoHome_Click(object sender, EventArgs e)
        {
            // 获取当前激活的窗口
            var currentWindow = GetCurrentActiveWindow();
            if (currentWindow == null)
            {
                LogHelper.LogError("没有激活的窗口");
                return;
            }

            // 激活窗口
            LogHelper.Log($"正在激活窗口: {currentWindow.Title}");
            _findWindowsAction.ActivateWindow(currentWindow);
            Thread.Sleep(500);

            // 刷新窗口坐标
            RefreshWindowPosition(currentWindow);

            // 创建回城动作
            GoHomeAction action = new GoHomeAction(_imageRec, _human, _window, _delay);

            // 订阅日志
            action.OnLog += (logMessage) => LogHelper.Log(logMessage);

            // 执行动作
            bool result = action.ExecuteWithWindow(currentWindow);

            if (result)
                LogHelper.LogSuccess("回城执行成功");
            else
                LogHelper.LogError("回城执行失败");
        }

        private void btnStep6_GoHome_Click(object sender, EventArgs e)
        {
            LogHelper.Log("执行动作6: 回城");
            // TODO: 实现回城逻辑
            LogHelper.LogSuccess("动作6完成");
        }

        private void btnNextWindow_Click(object sender, EventArgs e)
        {
            if (_foundWindows.Count == 0)
            {
                LogHelper.LogError("没有找到任何窗口");
                return;
            }

            // 找到当前激活的窗口（在选中的窗口列表中）
            int currentIndex = -1;
            for (int i = 0; i < _foundWindows.Count; i++)
            {
                if (_foundWindows[i].IsActive)
                {
                    currentIndex = i;
                    break;
                }
            }

            // 如果没找到激活的窗口，激活第一个
            if (currentIndex == -1)
            {
                _findWindowsAction.ActivateWindow(_foundWindows[0]);
                LogHelper.Log($"激活窗口: {_foundWindows[0].Title}");
                return;
            }

            // 计算下一个窗口索引（只在选中的窗口中循环）
            int nextIndex = (currentIndex + 1) % _foundWindows.Count;

            // 激活下一个窗口
            _findWindowsAction.ActivateWindow(_foundWindows[nextIndex]);
            LogHelper.Log($"切换到窗口 {nextIndex + 1}: {_foundWindows[nextIndex].Title}");
        }

        private void btnOpenTemplates_Click(object sender, EventArgs e)
        {
            try
            {
                // 图片模板文件夹路径
                string templatePath = Path.Combine(
                    Application.StartupPath,
                    "Images",
                    "Templates"
                );

                // 用资源管理器打开
                string systemExplorer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
                System.Diagnostics.Process.Start(systemExplorer, templatePath);

                LogHelper.Log($"已打开图片文件夹: {templatePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开文件夹失败: {ex.Message}", "错误");
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            LogHelper.Clear();
            LogHelper.Log("🧹 日志已清空", Color.Gray);
        }

        /// <summary>
        /// 自动运行按钮点击事件
        /// </summary>
        private async void btnAutoRun_Click(object sender, EventArgs e)
        {
            if (_isAutoRunning)
            {
                LogHelper.LogWarning("程序已在运行中");
                return;
            }

            // 检查是否有选中的窗口
            if (_foundWindows == null || _foundWindows.Count == 0)
            {
                LogHelper.LogError("没有选中的窗口，请重启程序并选择窗口");
                MessageBox.Show("没有选中的窗口，请重启程序并选择要采集的窗口", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 重置取消令牌
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            // 创建自动运行管理器
            AutoRunManager autoRun = new AutoRunManager(
                _imageRec,
                _human,
                _window,
                _delay,
                _findWindowsAction,
                (msg) => LogHelper.Log(msg),
                token,
                () => _isPaused
            );

            // 初始化 - 传入已选中的窗口
            if (!autoRun.Initialize(_foundWindows))
            {
                LogHelper.LogError("没有找到任何窗口");
                return;
            }

            _isAutoRunning = true;
            _isPaused = false;
            btnAutoRun.Enabled = false;
            btnPause.Enabled = true;
            btnStop.Enabled = true;
            btnPause.Text = "暂停运行";

            LogHelper.LogSuccess("=== 开始全自动运行 ===");

            int round = 1;

            // 运行循环
            while (_isAutoRunning && !token.IsCancellationRequested)
            {
                try
                {
                    // 检查暂停状态
                    while (_isPaused && _isAutoRunning && !token.IsCancellationRequested)
                    {
                        await Task.Delay(100);
                    }

                    if (!_isAutoRunning || token.IsCancellationRequested) break;

                    // 执行一轮采集
                    bool completed = await autoRun.ExecuteOneRoundAsync(round);

                    if (!completed)
                    {
                        if (_isAutoRunning && !token.IsCancellationRequested)
                        {
                            LogHelper.LogWarning("采集被中断");
                        }
                        break;
                    }

                    if (!_isAutoRunning || token.IsCancellationRequested) break;

                    // 记录本轮完成
                    LogHelper.LogRoundComplete(round);

                    // 等待5分钟
                    LogHelper.Log($"等待5分钟后开始下一轮...");

                    for (int i = 300; i > 0 && _isAutoRunning && !token.IsCancellationRequested; i--)
                    {
                        while (_isPaused && _isAutoRunning && !token.IsCancellationRequested)
                        {
                            await Task.Delay(100);
                        }

                        if (!_isAutoRunning || token.IsCancellationRequested) break;

                        if (i % 60 == 0 && i > 0)
                        {
                            LogHelper.Log($"还剩 {i / 60} 分钟...");
                        }

                        await Task.Delay(1000);
                    }

                    round++;
                }
                catch (OperationCanceledException)
                {
                    LogHelper.Log("自动运行已取消");
                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.LogError($"自动运行出错: {ex.Message}");
                    await Task.Delay(30000);
                }
            }

            // 清理
            _isAutoRunning = false;
            _isPaused = false;
            btnAutoRun.Enabled = true;
            btnPause.Enabled = false;
            btnStop.Enabled = false;
            LogHelper.Log("=== 自动运行已停止 ===");
        }

        /// <summary>
        /// 暂停/继续
        /// </summary>
        private void btnPause_Click(object sender, EventArgs e)
        {
            if (!_isAutoRunning) return;

            _isPaused = !_isPaused;
            btnPause.Text = _isPaused ? "继续运行" : "暂停运行";

            if (_isPaused)
                LogHelper.LogWarning("⏸️ 已暂停");
            else
                LogHelper.LogSuccess("▶️ 继续运行");
        }

        /// <summary>
        /// 停止 - 立即生效
        /// </summary>
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (!_isAutoRunning) return;

            LogHelper.LogWarning("⏹️ 正在停止自动运行...");

            // 取消令牌，立即停止
            _cancellationTokenSource?.Cancel();
            _isAutoRunning = false;
            _isPaused = false;
        }

        /// <summary>
        /// 窗体关闭时清理资源
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _isAutoRunning = false;
            base.OnFormClosing(e);
        }

        /// <summary>
        /// 选择窗口按钮 - 重新选择要采集的窗口
        /// </summary>
        private void btnSelectWindows_Click(object sender, EventArgs e)
        {
            // 如果正在自动运行，先提示
            if (_isAutoRunning)
            {
                LogHelper.LogWarning("请先停止自动运行，再选择窗口");
                MessageBox.Show("请先停止自动运行，再选择窗口", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LogHelper.Log("正在重新查找游戏窗口...", Color.Blue);

            // 重新查找所有窗口
            bool found = _findWindowsAction.Execute();

            if (found)
            {
                var allWindows = _findWindowsAction.GetFoundWindows();
                LogHelper.Log($"找到 {allWindows.Count} 个游戏窗口", Color.Green);

                // 显示窗口选择对话框
                using (var selector = new WindowSelectorForm(allWindows))
                {
                    var result = selector.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        // 更新选中的窗口
                        _foundWindows = selector.SelectedWindows;
                        LogHelper.LogSuccess($"已重新选择 {_foundWindows.Count} 个窗口：");
                        for (int i = 0; i < _foundWindows.Count; i++)
                        {
                            var w = _foundWindows[i];
                            LogHelper.Log($"  {i + 1}. {w.Title} 位置({w.X},{w.Y})", Color.Gray);
                        }

                        // 默认激活第一个选中的窗口
                        if (_foundWindows.Count > 0)
                        {
                            _findWindowsAction.ActivateWindow(_foundWindows[0]);
                            LogHelper.Log($"已激活窗口: {_foundWindows[0].Title}", Color.Green);
                        }
                    }
                    else
                    {
                        LogHelper.LogWarning("用户取消了窗口选择，窗口列表未变更");
                    }
                }
            }
            else
            {
                LogHelper.LogWarning("未找到任何游戏窗口");
                MessageBox.Show("未找到任何游戏窗口，请确保游戏已打开", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}