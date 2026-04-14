using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TianXiaMiner.Core;
using TianXiaMiner.Utils;

namespace TianXiaMiner.Actions
{
    /// <summary>
    /// 动作1：查找所有游戏窗体（多开版）
    /// 找到1-5个游戏窗口，记录每个窗口的位置和大小
    /// </summary>
    public class FindAllWindowsAction : BaseAction
    {
        // Windows API
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // 存储找到的窗口
        private List<GameWindowInfo> _foundWindows = new List<GameWindowInfo>();
        private string _searchTitle;

        /// <summary>
        /// 游戏窗口信息类
        /// </summary>
        public class GameWindowInfo
        {
            public IntPtr Hwnd { get; set; }           // 窗口句柄
            public string Title { get; set; }           // 窗口标题
            public int X { get; set; }                  // 左上角X
            public int Y { get; set; }                  // 左上角Y
            public int Width { get; set; }               // 窗口宽度
            public int Height { get; set; }              // 窗口高度
            public bool IsActive { get; set; }           // 是否当前激活

            public override string ToString()
            {
                return $"{Title} - ({X},{Y}) {Width}x{Height}";
            }
        }

        public FindAllWindowsAction(ImageRecognition imageRec, HumanSimulator human, WindowHelper window, GameDelay delay)
            : base(imageRec, human, window, delay)
        {
        }

        /// <summary>
        /// 执行查找所有匹配的游戏窗口
        /// </summary>
        public override bool Execute()
        {

            Log("开始查找所有游戏窗口");
            _foundWindows.Clear();

            // 从App.config读取窗口标题
            string windowTitle = System.Configuration.ConfigurationManager.AppSettings["GameWindowTitle"] ?? "天下3";
            _searchTitle = windowTitle;

            // 枚举所有窗口
            EnumWindows(new EnumWindowsProc(EnumWindowCallback), IntPtr.Zero);

            Log($"找到 {_foundWindows.Count} 个游戏窗口");

            // 输出每个窗口的信息
            for (int i = 0; i < _foundWindows.Count; i++)
            {
                var w = _foundWindows[i];
                Log($"窗口{i + 1}: {w.Title} 位置({w.X},{w.Y}) 大小{w.Width}x{w.Height}");
            }

            return _foundWindows.Count > 0;
        }

        /// <summary>
        /// 按指定标题查找窗口（不使用默认配置）
        /// </summary>
        public bool FindWindowsByTitle(string titlePart)
        {
            Log($"按标题查找: {titlePart}");
            _foundWindows.Clear();
            _searchTitle = titlePart;

            EnumWindows(new EnumWindowsProc(EnumWindowCallback), IntPtr.Zero);

            Log($"找到 {_foundWindows.Count} 个窗口");
            return _foundWindows.Count > 0;
        }

        /// <summary>
        /// 枚举窗口回调
        /// </summary>
        private bool EnumWindowCallback(IntPtr hWnd, IntPtr lParam)
        {
            // 只处理可见窗口
            if (!IsWindowVisible(hWnd))
                return true;

            // 获取窗口标题
            StringBuilder title = new StringBuilder(256);
            GetWindowText(hWnd, title, 256);

            string windowTitle = title.ToString();

            // 检查是否包含搜索的标题
            if (!string.IsNullOrEmpty(windowTitle) &&
                windowTitle.Contains(_searchTitle))
            {
                // 获取窗口位置和大小
                RECT rect = new RECT();
                if (GetWindowRect(hWnd, ref rect))
                {
                    _foundWindows.Add(new GameWindowInfo
                    {
                        Hwnd = hWnd,
                        Title = windowTitle,
                        X = rect.Left,
                        Y = rect.Top,
                        Width = rect.Right - rect.Left,
                        Height = rect.Bottom - rect.Top,
                        IsActive = false
                    });
                }
            }

            return true;
        }

        /// <summary>
        /// 获取找到的所有窗口
        /// </summary>
        public List<GameWindowInfo> GetFoundWindows()
        {
            return _foundWindows;
        }

        /// <summary>
        /// 激活指定窗口，并返回窗口信息
        /// </summary>
        public GameWindowInfo ActivateWindow(GameWindowInfo window)
        {
            if (window.Hwnd != IntPtr.Zero)
            {
                ShowWindow(window.Hwnd, SW_RESTORE);
                SetForegroundWindow(window.Hwnd);
                _delay.ActionDelay();

                // 刷新窗口位置（防止窗口被移动）
                RECT rect = new RECT();
                GetWindowRect(window.Hwnd, ref rect);

                // 更新窗口信息
                window.X = rect.Left;
                window.Y = rect.Top;
                window.Width = rect.Right - rect.Left;
                window.Height = rect.Bottom - rect.Top;
                window.IsActive = true;

                // 更新其他窗口的激活状态
                foreach (var w in _foundWindows)
                {
                    if (w.Hwnd != window.Hwnd)
                    {
                        w.IsActive = false;
                    }
                }

                Log($"激活窗口: {window.Title} 位置({window.X},{window.Y}) 大小{window.Width}x{window.Height}");
                return window;
            }
            return null;
        }

        /// <summary>
        /// 切换到下一个窗口
        /// </summary>
        public bool SwitchToNextWindow()
        {
            if (_foundWindows.Count == 0)
                return false;

            // 找到当前激活的窗口
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
                ActivateWindow(_foundWindows[0]);
                return true;
            }

            // 计算下一个窗口索引
            int nextIndex = (currentIndex + 1) % _foundWindows.Count;

            // 激活下一个窗口
            ActivateWindow(_foundWindows[nextIndex]);

            Log($"切换到窗口 {nextIndex + 1}: {_foundWindows[nextIndex].Title}");

            return true;
        }

        // 需要的Windows API
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref RECT rectangle);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}