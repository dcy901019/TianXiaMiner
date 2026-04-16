using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianXiaMiner.Core;
using TianXiaMiner.Actions;
using TianXiaMiner.Utils;

namespace TianXiaMiner
{
    /// <summary>
    /// 自动运行管理器 - 支持异步取消
    /// </summary>
    public class AutoRunManager
    {
        private ImageRecognition _imageRec;
        private HumanSimulator _human;
        private WindowHelper _window;
        private GameDelay _delay;
        private FindAllWindowsAction _findWindowsAction;
        private List<FindAllWindowsAction.GameWindowInfo> _selectedWindows;  // 改为使用已选窗口
        private Action<string> _logAction;

        // 取消和暂停标志
        private CancellationToken _cancellationToken;
        private Func<bool> _isPaused;

        // 配置
        private int _missingWindowDelay = 30;

        public AutoRunManager(
            ImageRecognition imageRec,
            HumanSimulator human,
            WindowHelper window,
            GameDelay delay,
            FindAllWindowsAction findWindowsAction,
            Action<string> logAction,
            CancellationToken cancellationToken,
            Func<bool> isPaused)
        {
            _imageRec = imageRec;
            _human = human;
            _window = window;
            _delay = delay;
            _findWindowsAction = findWindowsAction;
            _logAction = logAction;
            _cancellationToken = cancellationToken;
            _isPaused = isPaused;
            _selectedWindows = new List<FindAllWindowsAction.GameWindowInfo>();

            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                string delayStr = System.Configuration.ConfigurationManager.AppSettings["MissingWindowDelay"];
                if (!string.IsNullOrEmpty(delayStr) && int.TryParse(delayStr, out int delay))
                {
                    _missingWindowDelay = delay;
                }
            }
            catch { }
        }

        private void Log(string message)
        {
            _logAction?.Invoke(message);
        }

        /// <summary>
        /// 检查是否需要暂停或停止
        /// </summary>
        private async Task<bool> CheckPauseOrCancelAsync()
        {
            // 检查是否被取消
            if (_cancellationToken.IsCancellationRequested)
            {
                Log("⚠️ 检测到停止信号");
                return true;
            }

            // 检查是否暂停（暂停时循环等待）
            while (_isPaused != null && _isPaused())
            {
                await Task.Delay(100, _cancellationToken);
                if (_cancellationToken.IsCancellationRequested)
                {
                    Log("⚠️ 暂停期间收到停止信号");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 可取消的延迟
        /// </summary>
        private async Task<bool> DelayWithCancelAsync(int milliseconds)
        {
            try
            {
                await Task.Delay(milliseconds, _cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                Log("⚠️ 延迟被取消");
                return false;
            }
        }

        /// <summary>
        /// 初始化 - 使用已在 Main.cs 中筛选好的窗口列表
        /// </summary>
        public bool Initialize(List<FindAllWindowsAction.GameWindowInfo> selectedWindows)
        {
            Log("正在初始化窗口列表...");

            if (selectedWindows != null && selectedWindows.Count > 0)
            {
                _selectedWindows = selectedWindows;
                Log($"使用 {_selectedWindows.Count} 个已选窗口：");
                for (int i = 0; i < _selectedWindows.Count; i++)
                {
                    Log($"  {i + 1}. {_selectedWindows[i].Title} 位置({_selectedWindows[i].X},{_selectedWindows[i].Y})");
                }
                return true;
            }

            Log("错误：没有可用的窗口");
            return false;
        }

        /// <summary>
        /// 执行一轮采集
        /// </summary>
        public async Task<bool> ExecuteOneRoundAsync(int roundNumber)
        {
            if (_selectedWindows.Count == 0)
            {
                Log("错误：未初始化窗口列表");
                return false;
            }

            Log($"");
            Log($"=== 开始第 {roundNumber} 轮采集，已选窗口 {_selectedWindows.Count} 个 ===");

            for (int i = 0; i < _selectedWindows.Count; i++)
            {
                // 每处理一个窗口前检查暂停/停止
                if (await CheckPauseOrCancelAsync()) return false;

                var window = _selectedWindows[i];

                // 刷新窗口坐标（窗口可能被移动过）
                RefreshWindowPosition(window);

                // 检查窗口是否还存在（通过句柄）
                if (!IsWindowStillExists(window))
                {
                    Log($"窗口 {i + 1} ({window.Title}) 已关闭或掉线，等待 {_missingWindowDelay} 秒后继续...");
                    if (!await DelayWithCancelAsync(_missingWindowDelay * 1000)) return false;
                    continue;
                }

                Log($"");
                Log($"=== 正在处理窗口 {i + 1}/{_selectedWindows.Count}: {window.Title} ===");

                // 激活窗口
                _findWindowsAction.ActivateWindow(window);
                if (!await DelayWithCancelAsync(500)) return false;

                // 刷新窗口坐标（激活后再次刷新）
                RefreshWindowPosition(window);

                // 执行采集步骤
                if (!await ExecuteCollectionStepsAsync(window)) return false;

                // 回城判断
                bool needHome = ShouldGoHome(roundNumber, i);
                if (needHome)
                {
                    if (!await ExecuteGoHomeAsync(window)) return false;
                }

                // 不是最后一个窗口时，等待1秒
                if (i < _selectedWindows.Count - 1)
                {
                    Log("等待1秒后切换到下一个窗口...");
                    if (!await DelayWithCancelAsync(1000)) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查窗口是否还存在
        /// </summary>
        private bool IsWindowStillExists(FindAllWindowsAction.GameWindowInfo window)
        {
            if (window.Hwnd == IntPtr.Zero) return false;

            // 尝试获取窗口位置，如果失败说明窗口已不存在
            var rect = new WindowHelper.RECT();
            return WindowHelper.GetWindowRect(window.Hwnd, ref rect);
        }

        /// <summary>
        /// 执行采集步骤
        /// </summary>
        private async Task<bool> ExecuteCollectionStepsAsync(FindAllWindowsAction.GameWindowInfo window)
        {
            // 动作4: 采集资源
            Log($"--- 执行动作4: 采集资源 ---");
            if (await CheckPauseOrCancelAsync()) return false;

            CollectResourceAction action4 = new CollectResourceAction(_imageRec, _human, _window, _delay);
            action4.SetWindowPosition(window.X, window.Y, window.Width, window.Height);
            action4.OnLog += (logMessage) => Log(logMessage);
            action4.ExecuteWithWindow(window);

            if (!await DelayWithCancelAsync(500)) return false;

            // 动作2: 放入感应材料
            Log($"--- 执行动作2: 放入感应材料 ---");
            if (await CheckPauseOrCancelAsync()) return false;

            UseMaterialAction action2 = new UseMaterialAction(_imageRec, _human, _window, _delay);
            action2.SetWindowPosition(window.X, window.Y, window.Width, window.Height);
            action2.OnLog += (logMessage) => Log(logMessage);
            action2.ExecuteWithWindow(window);

            if (!await DelayWithCancelAsync(500)) return false;

            // 动作3: 地图找资源
            Log($"--- 执行动作3: 地图找资源 ---");
            if (await CheckPauseOrCancelAsync()) return false;

            UseMapAction action3 = new UseMapAction(_imageRec, _human, _window, _delay);
            action3.SetWindowPosition(window.X, window.Y, window.Width, window.Height);
            action3.OnLog += (logMessage) => Log(logMessage);
            action3.Execute(window);

            if (!await DelayWithCancelAsync(500)) return false;

            return true;
        }

        /// <summary>
        /// 执行回城
        /// </summary>
        private async Task<bool> ExecuteGoHomeAsync(FindAllWindowsAction.GameWindowInfo window)
        {
            Log($"--- 执行动作6: 回城 (轮次回城) ---");
            if (await CheckPauseOrCancelAsync()) return false;

            GoHomeAction action6 = new GoHomeAction(_imageRec, _human, _window, _delay);
            action6.SetWindowPosition(window.X, window.Y, window.Width, window.Height);
            action6.OnLog += (logMessage) => Log(logMessage);
            action6.ExecuteWithWindow(window);

            if (!await DelayWithCancelAsync(500)) return false;

            return true;
        }

        /// <summary>
        /// 判断是否需要回城
        /// 规则：第一轮都不回，之后每轮只有一个窗口回城，轮流进行
        /// </summary>
        private bool ShouldGoHome(int roundNumber, int windowIndex)
        {
            if (roundNumber == 1) return false;
            int homeIndex = (roundNumber - 2) % _selectedWindows.Count;
            return windowIndex == homeIndex;
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
    }
}