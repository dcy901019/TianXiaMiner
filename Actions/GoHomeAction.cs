using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianXiaMiner.Core;
using TianXiaMiner.Utils;

namespace TianXiaMiner.Actions
{
    /// <summary>
    /// 动作6：回城
    /// 按下小键盘4，等待10-11秒
    /// </summary>
    public class GoHomeAction : BaseAction
    {
        // 回城快捷键
        private Keys _homeKey = Keys.NumPad4;

        // 等待时间范围（秒）
        private int _minWaitSeconds = 10;
        private int _maxWaitSeconds = 11;

        public GoHomeAction(ImageRecognition imageRec, HumanSimulator human, WindowHelper window, GameDelay delay)
            : base(imageRec, human, window, delay)
        {
            // 尝试从配置文件读取等待时间
            LoadConfig();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                string minWaitStr = System.Configuration.ConfigurationManager.AppSettings["GoHomeMinWait"];
                if (!string.IsNullOrEmpty(minWaitStr) && int.TryParse(minWaitStr, out int minWait))
                {
                    _minWaitSeconds = minWait;
                }

                string maxWaitStr = System.Configuration.ConfigurationManager.AppSettings["GoHomeMaxWait"];
                if (!string.IsNullOrEmpty(maxWaitStr) && int.TryParse(maxWaitStr, out int maxWait))
                {
                    _maxWaitSeconds = maxWait;
                }

                Log($"回城配置加载成功：等待时间 {_minWaitSeconds}-{_maxWaitSeconds} 秒");
            }
            catch (Exception ex)
            {
                Log($"加载配置文件出错: {ex.Message}，使用默认值 10-11 秒");
            }
        }

        /// <summary>
        /// 必须实现的抽象方法
        /// </summary>
        public override bool Execute()
        {
            Log("错误：请使用带窗口参数的 Execute 方法");
            return false;
        }

        /// <summary>
        /// 执行动作（带窗口参数）
        /// </summary>
        public bool ExecuteWithWindow(FindAllWindowsAction.GameWindowInfo window)
        {
            SetWindowPosition(window.X, window.Y, window.Width, window.Height);
            return ExecuteInternal();
        }

        /// <summary>
        /// 实际的执行逻辑
        /// </summary>
        private bool ExecuteInternal()
        {
            Log("=== 动作6: 回城 ===");

            try
            {
                // 按下小键盘4
                Log($"按下小键盘4回城");
                _human.PressKey(_homeKey);

                // 计算随机等待时间（毫秒）
                int waitMs = _random.Next(_minWaitSeconds * 1000, _maxWaitSeconds * 1000);
                int waitSeconds = waitMs / 1000;

                Log($"等待 {waitSeconds} 秒，让角色回城...");

                // 使用 Thread.Sleep 等待（不用 GameDelay 因为需要精确的秒数）
                Thread.Sleep(waitMs);

                Log("✅ 回城完成");
                Log("=== 动作6完成 ===");

                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ 执行出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 设置回城快捷键（默认是小键盘4）
        /// </summary>
        public void SetHomeKey(Keys key)
        {
            _homeKey = key;
            Log($"回城快捷键已设置为: {key}");
        }

        /// <summary>
        /// 设置等待时间范围（秒）
        /// </summary>
        public void SetWaitTime(int minSeconds, int maxSeconds)
        {
            _minWaitSeconds = minSeconds;
            _maxWaitSeconds = maxSeconds;
            Log($"回城等待时间已设置为: {minSeconds}-{maxSeconds} 秒");
        }
    }
}