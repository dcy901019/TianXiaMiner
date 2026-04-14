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

            // 设置文本框支持滚轮
            txtLog.MouseWheel += TxtLog_MouseWheel;

            // 初始化取消令牌
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 文本框滚轮事件 - 实现上下滚动
        /// </summary>
        private void TxtLog_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                // 向上滚动
                if (txtLog.SelectionStart > 0)
                {
                    int linesToScroll = Math.Min(3, txtLog.SelectionStart);
                    txtLog.SelectionStart = Math.Max(0, txtLog.SelectionStart - linesToScroll);
                    txtLog.ScrollToCaret();
                }
            }
            else
            {
                // 向下滚动
                if (txtLog.SelectionStart < txtLog.Text.Length)
                {
                    int linesToScroll = Math.Min(3, txtLog.Text.Length - txtLog.SelectionStart);
                    txtLog.SelectionStart = Math.Min(txtLog.Text.Length, txtLog.SelectionStart + linesToScroll);
                    txtLog.ScrollToCaret();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 初始化日志控件
            LogHelper.Initialize(txtLog);

            LogHelper.Log("=== 程序启动，自动查找窗口 ===");

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
                    LogHelper.Log($"创建文件夹: {folder}");
                }
            }

            // 从配置文件读取标题
            string configTitle = System.Configuration.ConfigurationManager.AppSettings["GameWindowTitle"] ?? "天下3";
            LogHelper.Log($"查找标题: {configTitle}");

            // 执行查找
            bool found = _findWindowsAction.Execute();

            if (found)
            {
                _foundWindows = _findWindowsAction.GetFoundWindows();
                LogHelper.Log($"找到 {_foundWindows.Count} 个游戏窗口：");
                for (int i = 0; i < _foundWindows.Count; i++)
                {
                    var w = _foundWindows[i];
                    LogHelper.Log($"  {i + 1}. {w.Title}");
                }

                // 默认激活第一个窗口
                if (_foundWindows.Count > 0)
                {
                    _findWindowsAction.ActivateWindow(_foundWindows[0]);
                    LogHelper.Log($"已激活窗口: {_foundWindows[0].Title}");
                }
            }
            else
            {
                LogHelper.Log("未找到游戏窗口，请手动打开游戏");
            }

            LogHelper.Log("=== 初始化完成 ===");

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
                LogHelper.Log("错误：没有激活的窗口");
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
            LogHelper.Log(result ? "检测足通成功" : "检测足通失败");
        }

        private void btnStep2_UseMaterial_Click(object sender, EventArgs e)
        {
            // 获取当前激活的窗口
            var currentWindow = GetCurrentActiveWindow();
            if (currentWindow == null)
            {
                LogHelper.Log("错误：没有激活的窗口");
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

            // 【重要】订阅日志事件，把Action里的日志显示到界面上
            action.OnLog += (logMessage) => LogHelper.Log(logMessage);

            LogHelper.Log($"准备执行动作2，窗口: {currentWindow.Title} 位置({currentWindow.X},{currentWindow.Y})");

            // 动作内部自己处理所有配置和日志
            bool result = action.ExecuteWithWindow(currentWindow);

            LogHelper.Log(result ? "动作2执行成功" : "动作2执行失败");
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
            LogHelper.Log("=== 动作3: 地图找资源 ===");

            // 获取当前激活的窗口
            var currentWindow = GetCurrentActiveWindow();
            if (currentWindow == null)
            {
                LogHelper.Log("错误：没有激活的窗口");
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

            LogHelper.Log(result ? "动作3执行成功" : "动作3执行失败");
        }

        private void btnStep4_CollectResource_Click(object sender, EventArgs e)
        {
            var currentWindow = GetCurrentActiveWindow();
            if (currentWindow == null)
            {
                LogHelper.Log("错误：没有激活的窗口");
                return;
            }

            LogHelper.Log($"正在激活窗口: {currentWindow.Title}");
            _findWindowsAction.ActivateWindow(currentWindow);
            Thread.Sleep(500);

            RefreshWindowPosition(currentWindow);

            CollectResourceAction action = new CollectResourceAction(_imageRec, _human, _window, _delay);
            action.OnLog += (logMessage) => LogHelper.Log(logMessage);

            bool result = action.ExecuteWithWindow(currentWindow);
            LogHelper.Log(result ? "动作4执行成功" : "动作4执行失败");
        }

        private void btnStep5_GoHome_Click(object sender, EventArgs e)
        {
            // 获取当前激活的窗口
            var currentWindow = GetCurrentActiveWindow();
            if (currentWindow == null)
            {
                LogHelper.Log("错误：没有激活的窗口");
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

            LogHelper.Log(result ? "回城执行成功" : "回城执行失败");
        }

        private void btnStep6_GoHome_Click(object sender, EventArgs e)
        {
            LogHelper.Log("执行动作6: 回城");
            // TODO: 实现回城逻辑
            LogHelper.Log("动作6完成");
        }

        private void btnNextWindow_Click(object sender, EventArgs e)
        {
            if (_foundWindows.Count == 0)
            {
                LogHelper.Log("错误：没有找到任何窗口");
                return;
            }

            bool switched = _findWindowsAction.SwitchToNextWindow();

            if (switched)
            {
                var windows = _findWindowsAction.GetFoundWindows();
                for (int i = 0; i < windows.Count; i++)
                {
                    if (windows[i].IsActive)
                    {
                        LogHelper.Log($"切换到窗口 {i + 1}: {windows[i].Title}");
                        break;
                    }
                }
            }
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

              
               

                // 用资源管理器打开（使用完整系统路径）
                string systemExplorer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
                System.Diagnostics.Process.Start(systemExplorer, templatePath);

                LogHelper.Log($"已打开图片文件夹: {templatePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开文件夹失败: {ex.Message}", "错误");
            }
        }

        /// <summary>
        /// 自动运行按钮点击事件
        /// </summary>
        private async void btnAutoRun_Click(object sender, EventArgs e)
        {
            if (_isAutoRunning)
            {
                LogHelper.Log("程序已在运行中");
                return;
            }

            // 创建自动运行管理器
            AutoRunManager autoRun = new AutoRunManager(
                _imageRec,
                _human,
                _window,
                _delay,
                _findWindowsAction,
                (msg) => LogHelper.Log(msg)
            );

            // 初始化（记录原始窗口）
            if (!autoRun.Initialize())
            {
                LogHelper.Log("错误：没有找到任何窗口");
                return;
            }

            // 重置取消令牌
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _isAutoRunning = true;
            _isPaused = false;
            btnAutoRun.Enabled = false;
            btnPause.Enabled = true;
            btnStop.Enabled = true;
            btnPause.Text = "暂停运行";

            LogHelper.Log($"=== 开始全自动运行 ===");

            int round = 1;

            // 在后台线程运行，避免阻塞UI
            await Task.Run(async () =>
            {
                while (_isAutoRunning && !token.IsCancellationRequested)
                {
                    try
                    {
                        // 检查暂停状态
                        while (_isPaused && !token.IsCancellationRequested)
                        {
                            await Task.Delay(1000, token);
                        }

                        if (token.IsCancellationRequested) break;

                        // 执行一轮采集
                        autoRun.ExecuteOneRound(round);

                        if (token.IsCancellationRequested) break;

                        // 记录本轮完成
                        LogHelper.LogRoundComplete(round);

                        // 一轮结束后，等待5分钟（支持暂停和取消）
                        LogHelper.Log($"等待5分钟后开始下一轮...");

                        for (int i = 300; i > 0 && _isAutoRunning && !token.IsCancellationRequested; i--)
                        {
                            // 检查暂停状态
                            while (_isPaused && !token.IsCancellationRequested)
                            {
                                await Task.Delay(1000, token);
                            }

                            if (i % 60 == 0)
                            {
                                LogHelper.Log($"还剩 {i / 60} 分钟...");
                            }

                            // 使用可取消的延迟
                            await Task.Delay(1000, token);
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
                        LogHelper.Log($"❌ 自动运行出错: {ex.Message}");
                        LogHelper.Log("等待30秒后继续...");

                        for (int i = 30; i > 0 && _isAutoRunning && !token.IsCancellationRequested; i--)
                        {
                            await Task.Delay(1000, token);
                        }
                    }
                }
            }, token);

            // 运行结束后的清理
            _isAutoRunning = false;
            btnAutoRun.Enabled = true;
            btnPause.Enabled = false;
            btnStop.Enabled = false;
            LogHelper.Log("=== 自动运行已停止 ===");
        }

        /// <summary>
        /// 暂停/继续按钮点击事件
        /// </summary>
        private void btnPause_Click(object sender, EventArgs e)
        {
            if (!_isAutoRunning) return;

            _isPaused = !_isPaused;
            btnPause.Text = _isPaused ? "继续运行" : "暂停运行";
            LogHelper.Log(_isPaused ? "⏸️ 已暂停" : "▶️ 继续运行");
        }

        /// <summary>
        /// 停止按钮点击事件
        /// </summary>
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (!_isAutoRunning) return;

            // 发送取消信号
            _cancellationTokenSource?.Cancel();
            _isAutoRunning = false;
            _isPaused = false;

            LogHelper.Log("⏹️ 正在停止自动运行...");
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
    }
}