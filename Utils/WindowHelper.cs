using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TianXiaMiner.Utils
{
    /// <summary>
    /// 窗口辅助类 - 专门处理游戏窗口相关操作
    /// </summary>
    public class WindowHelper
    {
        // Windows API函数
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, ref RECT rectangle);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const int SW_RESTORE = 9;

        // 游戏窗口信息
        private IntPtr _gameHwnd = IntPtr.Zero;
        private RECT _gameRect;
        private bool _isWindowFound = false;

        /// <summary>
        /// 游戏窗口左上角X坐标
        /// </summary>
        public int GameX => _gameRect.Left;

        /// <summary>
        /// 游戏窗口左上角Y坐标
        /// </summary>
        public int GameY => _gameRect.Top;

        /// <summary>
        /// 游戏窗口宽度
        /// </summary>
        public int GameWidth => _gameRect.Right - _gameRect.Left;

        /// <summary>
        /// 游戏窗口高度
        /// </summary>
        public int GameHeight => _gameRect.Bottom - _gameRect.Top;

        /// <summary>
        /// 是否找到游戏窗口
        /// </summary>
        public bool IsWindowFound => _isWindowFound;

        /// <summary>
        /// 窗口句柄
        /// </summary>
        public IntPtr GameHwnd => _gameHwnd;

        /// <summary>
        /// 查找游戏窗口
        /// </summary>
        public bool FindGameWindow(string windowTitle)
        {
            // 方法1：精确查找
            _gameHwnd = FindWindow(null, windowTitle);

            // 方法2：遍历进程查找
            if (_gameHwnd == IntPtr.Zero)
            {
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (!string.IsNullOrEmpty(process.MainWindowTitle) &&
                        process.MainWindowTitle.Contains(windowTitle))
                    {
                        _gameHwnd = process.MainWindowHandle;
                        break;
                    }
                }
            }

            // 获取窗口位置
            if (_gameHwnd != IntPtr.Zero)
            {
                _isWindowFound = GetWindowRect(_gameHwnd, ref _gameRect);
                return _isWindowFound;
            }

            _isWindowFound = false;
            return false;
        }

        /// <summary>
        /// 激活游戏窗口
        /// </summary>
        public void ActivateWindow()
        {
            if (_gameHwnd != IntPtr.Zero)
            {
                ShowWindow(_gameHwnd, SW_RESTORE);
                SetForegroundWindow(_gameHwnd);
                System.Threading.Thread.Sleep(500);
            }
        }

        /// <summary>
        /// 刷新窗口位置
        /// </summary>
        public void RefreshWindowPosition()
        {
            if (_gameHwnd != IntPtr.Zero)
            {
                _isWindowFound = GetWindowRect(_gameHwnd, ref _gameRect);
            }
        }

        /// <summary>
        /// 获取窗口的进程ID
        /// </summary>
        public uint GetProcessId()
        {
            if (_gameHwnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(_gameHwnd, out uint pid);
                return pid;
            }
            return 0;
        }
    }
}