using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace TianXiaMiner.Core
{
    /// <summary>
    /// 人类行为模拟器 - 使用 InputSimulator
    /// </summary>
    public class HumanSimulator
    {
        private InputSimulator _inputSimulator = new InputSimulator();
        private Random _random = new Random();
        private GameDelay _delay = new GameDelay();
        private Point _lastMousePosition;

        // 按键扫描码（用于keybd_event）
        private Dictionary<Keys, byte> _scanCodes = new Dictionary<Keys, byte>
        {
            // 小键盘按键
            { Keys.NumPad0, 0x52 },
            { Keys.NumPad1, 0x4F },
            { Keys.NumPad2, 0x50 },
            { Keys.NumPad3, 0x51 },
            { Keys.NumPad4, 0x4B },
            { Keys.NumPad5, 0x4C },
            { Keys.NumPad6, 0x4D },
            { Keys.NumPad7, 0x47 },
            { Keys.NumPad8, 0x48 },
            { Keys.NumPad9, 0x49 },
            
            // 字母按键
            { Keys.B, 0x30 },    // B键
            { Keys.M, 0x32 },    // M键
            { Keys.V, 0x2F },    // V键
            { Keys.Z, 0x2C },    // Z键
            
            // 功能键
            { Keys.Escape, 0x01 },  // ESC键
            { Keys.Enter, 0x1C },   // 回车键
            { Keys.Space, 0x39 },   // 空格键
            
            // 修饰键
            { Keys.ShiftKey, 0x2A }, // Shift键
            { Keys.ControlKey, 0x1D }, // Ctrl键
            { Keys.Menu, 0x38 }      // Alt键
        };

        // Windows API for keybd_event
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public HumanSimulator()
        {
            _lastMousePosition = Cursor.Position;
        }

        #region 鼠标移动

        /// <summary>
        /// 像真人一样移动鼠标到目标位置
        /// </summary>
        public void MoveMouseLikeHuman(Point target, int gameX = 0, int gameY = 0)
        {
            Point absoluteTarget = new Point(target.X + gameX, target.Y + gameY);
            Point current = Cursor.Position;

            double distance = Math.Sqrt(Math.Pow(absoluteTarget.X - current.X, 2) +
                                        Math.Pow(absoluteTarget.Y - current.Y, 2));

            double baseTime = distance / 700.0;
            double moveTime = baseTime * _random.NextDouble() * 0.2 + baseTime * 0.9;
            if (moveTime < 0.08) moveTime = 0.08;
            if (moveTime > 1.2) moveTime = 1.2;

            int steps = (int)(moveTime * 40);
            if (steps < 3) steps = 3;
            if (steps > 30) steps = 30;

            for (int i = 1; i <= steps; i++)
            {
                double t = i / (double)steps;
                double easeT = 1 - Math.Pow(1 - t, 3);

                int currentX = (int)(current.X + (absoluteTarget.X - current.X) * easeT);
                int currentY = (int)(current.Y + (absoluteTarget.Y - current.Y) * easeT);

                int jitterX = steps > 10 ? _random.Next(-1, 2) : 0;
                int jitterY = steps > 10 ? _random.Next(-1, 2) : 0;

                _inputSimulator.Mouse.MoveMouseTo(
                    (currentX + jitterX) * 65535 / Screen.PrimaryScreen.Bounds.Width,
                    (currentY + jitterY) * 65535 / Screen.PrimaryScreen.Bounds.Height);

                int stepDelay = (int)(moveTime * 1000 / steps);
                Thread.Sleep(stepDelay);
            }

            int finalOffsetX = _random.Next(-2, 3);
            int finalOffsetY = _random.Next(-2, 3);

            _inputSimulator.Mouse.MoveMouseTo(
                (absoluteTarget.X + finalOffsetX) * 65535 / Screen.PrimaryScreen.Bounds.Width,
                (absoluteTarget.Y + finalOffsetY) * 65535 / Screen.PrimaryScreen.Bounds.Height);

            _lastMousePosition = new Point(absoluteTarget.X + finalOffsetX, absoluteTarget.Y + finalOffsetY);
        }

        #endregion

        #region 鼠标点击

        public void LeftClick()
        {
            _delay.OperationDelay();
            _inputSimulator.Mouse.LeftButtonClick();
            _delay.OperationDelay();
        }

        public void RightClick()
        {
            _delay.OperationDelay();
            _inputSimulator.Mouse.RightButtonClick();
            _delay.OperationDelay();
        }

        public void DoubleClick()
        {
            LeftClick();
            _delay.ActionDelay();
            LeftClick();
        }

        #endregion

        #region 键盘操作

        /// <summary>
        /// 按下一个键（带扫描码，确保游戏识别）
        /// </summary>
        public void PressKey(Keys key)
        {
            _delay.OperationDelay();

            // 方法1：InputSimulator
            VirtualKeyCode vk = (VirtualKeyCode)key;
            _inputSimulator.Keyboard.KeyPress(vk);

            // 方法2：keybd_event带扫描码（对小键盘特别有效）
            if (_scanCodes.ContainsKey(key))
            {
                byte vkByte = (byte)key;
                byte scanCode = _scanCodes[key];

                // 按下（带扫描码）
                keybd_event(vkByte, scanCode, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                // 抬起
                keybd_event(vkByte, scanCode, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }

            _delay.OperationDelay();
        }

        /// <summary>
        /// 按住一个键（用于移动）
        /// </summary>
        public void HoldKey(Keys key, int durationMs)
        {
            VirtualKeyCode vk = (VirtualKeyCode)key;
            _inputSimulator.Keyboard.KeyDown(vk);

            _delay.CustomDelay(durationMs - 50, durationMs + 50);

            _inputSimulator.Keyboard.KeyUp(vk);
        }

        /// <summary>
        /// 松开一个键
        /// </summary>
        public void ReleaseKey(Keys key)
        {
            VirtualKeyCode vk = (VirtualKeyCode)key;
            _inputSimulator.Keyboard.KeyUp(vk);
        }

        /// <summary>
        /// 按组合键（如 Shift+Z）
        /// </summary>
        public void PressCombination(Keys modifier, Keys key)
        {
            _delay.OperationDelay();

            VirtualKeyCode modVk = (VirtualKeyCode)modifier;
            VirtualKeyCode keyVk = (VirtualKeyCode)key;

            // 使用 InputSimulator 按组合键
            _inputSimulator.Keyboard.ModifiedKeyStroke(modVk, keyVk);

            // 同时用 keybd_event 带扫描码再按一次（确保游戏识别）
            if (_scanCodes.ContainsKey(modifier) && _scanCodes.ContainsKey(key))
            {
                byte modByte = (byte)modifier;
                byte modScan = _scanCodes[modifier];
                byte keyByte = (byte)key;
                byte keyScan = _scanCodes[key];

                // 按下修饰键
                keybd_event(modByte, modScan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                _delay.OperationDelay();

                // 按下主键
                keybd_event(keyByte, keyScan, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                _delay.OperationDelay();

                // 抬起主键
                keybd_event(keyByte, keyScan, KEYEVENTF_KEYUP, UIntPtr.Zero);
                _delay.OperationDelay();

                // 抬起修饰键
                keybd_event(modByte, modScan, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }

            _delay.OperationDelay();
        }

        /// <summary>
        /// 按三个键的组合（如 Ctrl+Shift+Esc）
        /// </summary>
        public void PressCombination(Keys modifier1, Keys modifier2, Keys key)
        {
            _delay.OperationDelay();

            VirtualKeyCode mod1Vk = (VirtualKeyCode)modifier1;
            VirtualKeyCode mod2Vk = (VirtualKeyCode)modifier2;
            VirtualKeyCode keyVk = (VirtualKeyCode)key;

            _inputSimulator.Keyboard.ModifiedKeyStroke(new[] { mod1Vk, mod2Vk }, keyVk);

            _delay.OperationDelay();
        }

        #endregion

        #region 行为组合

        /// <summary>
        /// 点击物体（移动+点击+动作延迟）
        /// </summary>
        public void ClickOn(Point target, int gameX = 0, int gameY = 0, bool rightButton = false)
        {
            MoveMouseLikeHuman(target, gameX, gameY);
            _delay.ActionDelay();

            if (rightButton)
                RightClick();
            else
                LeftClick();

            _delay.ActionDelay();
        }

        #endregion
    }
}