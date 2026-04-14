using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianXiaMiner.Core;
using TianXiaMiner.Utils;

namespace TianXiaMiner.Actions
{
    /// <summary>
    /// 动作基类 - 所有具体动作都继承这个类
    /// </summary>
    public abstract class BaseAction
    {
        // 核心功能类
        protected ImageRecognition _imageRec;
        protected HumanSimulator _human;
        protected WindowHelper _window;
        protected GameDelay _delay;
        protected Random _random;

        // 游戏窗口坐标原点
        protected int _gameX = 0;
        protected int _gameY = 0;
        // 添加窗口宽度和高度
        protected int _windowWidth = 0;
        protected int _windowHeight = 0;
        public BaseAction(ImageRecognition imageRec, HumanSimulator human, WindowHelper window, GameDelay delay)
        {
            _imageRec = imageRec;
            _human = human;
            _window = window;
            _delay = delay;
            _random = new Random();

            // 如果窗口已找到，获取坐标
            if (_window.IsWindowFound)
            {
                _gameX = _window.GameX;
                _gameY = _window.GameY;
            }
        }
        /// <summary>
        /// 设置窗口位置和大小
        /// </summary>
        public void SetWindowPosition(int x, int y, int width, int height)
        {
            _gameX = x;
            _gameY = y;
            _windowWidth = width;
            _windowHeight = height;
        }


        /// <summary>
        /// 更新窗口坐标（窗口移动后调用）
        /// </summary>
        public void UpdateWindowPosition()
        {
            _window.RefreshWindowPosition();
            _gameX = _window.GameX;
            _gameY = _window.GameY;
        }

        /// <summary>
        /// 执行动作（每个子类必须实现）
        /// </summary>
        public abstract bool Execute();


        public event Action<string> OnLog;
        /// <summary>
        /// 记录日志
        /// </summary>
        protected void Log(string message)
        {

            string timeStr = DateTime.Now.ToString("HH:mm:ss");
            string logMessage = $"[{timeStr}] {message}";

            // 输出到调试窗口
            System.Diagnostics.Debug.WriteLine(logMessage);

            // 触发事件，让 Form1 显示到 lstLog
            OnLog?.Invoke(logMessage);
        }
    }
}