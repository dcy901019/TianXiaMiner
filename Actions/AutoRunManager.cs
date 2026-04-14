using System;
using System.Collections.Generic;
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
    /// <summary>
    /// 自动运行管理器 - 简化版
    /// </summary>
    public class AutoRunManager
    {
        private ImageRecognition _imageRec;
        private HumanSimulator _human;
        private WindowHelper _window;
        private GameDelay _delay;
        private FindAllWindowsAction _findWindowsAction;
        private List<FindAllWindowsAction.GameWindowInfo> _originalWindows;  // 原始窗口列表
        private Action<string> _logAction;

        // 配置
        private int _missingWindowDelay = 30;  // 窗口丢失等待时间（秒）

        public AutoRunManager(
            ImageRecognition imageRec,
            HumanSimulator human,
            WindowHelper window,
            GameDelay delay,
            FindAllWindowsAction findWindowsAction,
            Action<string> logAction)
        {
            _imageRec = imageRec;
            _human = human;
            _window = window;
            _delay = delay;
            _findWindowsAction = findWindowsAction;
            _logAction = logAction;
            _originalWindows = new List<FindAllWindowsAction.GameWindowInfo>();

            // 读取配置文件
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
        /// 初始化 - 记录原始窗口
        /// </summary>
        public bool Initialize()
        {
            Log("正在初始化窗口列表...");

            bool found = _findWindowsAction.Execute();
            if (found)
            {
                _originalWindows = _findWindowsAction.GetFoundWindows().ToList();
                Log($"找到 {_originalWindows.Count} 个原始窗口：");
                for (int i = 0; i < _originalWindows.Count; i++)
                {
                    Log($"  {i + 1}. {_originalWindows[i].Title}");
                }
                return true;
            }

            Log("未找到任何窗口");
            return false;
        }

        /// <summary>
        /// 获取当前在线窗口
        /// </summary>
        private List<FindAllWindowsAction.GameWindowInfo> GetCurrentWindows()
        {
            _findWindowsAction.Execute();
            return _findWindowsAction.GetFoundWindows();
        }

        /// <summary>
        /// 执行一轮采集
        /// </summary>
        public void ExecuteOneRound(int roundNumber)
        {
            if (_originalWindows.Count == 0)
            {
                Log("错误：未初始化原始窗口列表");
                return;
            }

            Log($"");
            Log($"=== 开始第 {roundNumber} 轮采集，原始窗口 {_originalWindows.Count} 个 ===");

            // 按原始窗口顺序遍历
            for (int i = 0; i < _originalWindows.Count; i++)
            {
                var originalWindow = _originalWindows[i];

                // 获取当前在线窗口
                var currentWindows = GetCurrentWindows();
                var currentWindow = currentWindows.FirstOrDefault(w => w.Hwnd == originalWindow.Hwnd);

                // 检查窗口是否还在
                if (currentWindow == null)
                {
                    Log($"窗口 {i + 1} 已掉线，等待 {_missingWindowDelay} 秒后继续...");
                    Thread.Sleep(_missingWindowDelay * 1000);
                    continue;  // 跳过这个窗口，处理下一个
                }

                // 窗口在线，执行采集
                Log($"");
                Log($"=== 正在处理窗口 {i + 1}/{_originalWindows.Count}: {currentWindow.Title} ===");

                // 激活窗口
                _findWindowsAction.ActivateWindow(currentWindow);
                Thread.Sleep(1000);

                // 刷新窗口坐标
                RefreshWindowPosition(currentWindow);

                // 执行采集步骤
                ExecuteCollectionSteps(currentWindow);

                // 回城判断（轮次回城）
                bool needHome = ShouldGoHome(roundNumber, i);
                if (needHome)
                {
                    ExecuteGoHome(currentWindow);
                }

                // 不是最后一个窗口时，等待3秒再切下一个
                if (i < _originalWindows.Count - 1)
                {
                    Log("等待3秒后切换到下一个窗口...");
                    Thread.Sleep(3000);
                }
            }
        }

        /// <summary>
        /// 执行采集步骤
        /// </summary>
        private void ExecuteCollectionSteps(FindAllWindowsAction.GameWindowInfo window)
        {
            // 动作4: 采集资源
            Log($"--- 执行动作4: 采集资源 ---");
            CollectResourceAction action4 = new CollectResourceAction(_imageRec, _human, _window, _delay);
            action4.SetWindowPosition(window.X, window.Y, window.Width, window.Height);
            action4.OnLog += (logMessage) => Log(logMessage);
            action4.ExecuteWithWindow(window);
            Thread.Sleep(1000);

            // 动作2: 放入感应材料
            Log($"--- 执行动作2: 放入感应材料 ---");
            UseMaterialAction action2 = new UseMaterialAction(_imageRec, _human, _window, _delay);
            action2.SetWindowPosition(window.X, window.Y, window.Width, window.Height);
            action2.OnLog += (logMessage) => Log(logMessage);
            action2.ExecuteWithWindow(window);
            Thread.Sleep(1000);

            // 动作3: 地图找资源
            Log($"--- 执行动作3: 地图找资源 ---");
            UseMapAction action3 = new UseMapAction(_imageRec, _human, _window, _delay);
            action3.SetWindowPosition(window.X, window.Y, window.Width, window.Height);
            action3.OnLog += (logMessage) => Log(logMessage);
            action3.Execute(window);
            Thread.Sleep(1000);
        }

        /// <summary>
        /// 执行回城
        /// </summary>
        private void ExecuteGoHome(FindAllWindowsAction.GameWindowInfo window)
        {
            Log($"--- 执行动作6: 回城 (轮次回城) ---");
            GoHomeAction action6 = new GoHomeAction(_imageRec, _human, _window, _delay);
            action6.SetWindowPosition(window.X, window.Y, window.Width, window.Height);
            action6.OnLog += (logMessage) => Log(logMessage);
            action6.ExecuteWithWindow(window);
            Thread.Sleep(1000);
        }

        /// <summary>
        /// 判断是否需要回城
        /// 规则：第一轮都不回，之后每轮只有一个窗口回城，轮流进行
        /// </summary>
        private bool ShouldGoHome(int roundNumber, int windowIndex)
        {
            if (roundNumber == 1) return false;

            // 回城窗口索引 = (roundNumber - 2) % 窗口数量
            int homeIndex = (roundNumber - 2) % _originalWindows.Count;
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