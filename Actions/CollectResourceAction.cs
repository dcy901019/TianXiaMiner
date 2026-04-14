using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianXiaMiner.Core;
using TianXiaMiner.Utils;

namespace TianXiaMiner.Actions
{
    /// <summary>
    /// 动作4：采集资源
    /// 1. 按三下ESC清屏
    /// 2. 按小键盘1打开采集页面
    /// 3. 检查【资源放弃】图片是否存在
    ///    - 存在 → 资源还在，执行采集
    ///    - 不存在 → 资源已采完，结束
    /// 4. 每次采集前按一下ESC
    /// 5. 按 V+Shift+Z 组合键采集（次数可配置）
    /// 6. 循环执行指定轮次
    /// </summary>
    public class CollectResourceAction : BaseAction
    {
        // 采集页面图片
        private string _collectPageImage = "采集地图页面";

        // 资源放弃按钮图片（存在表示资源还在）
        private string _resourceCancelImage = "资源放弃";

        // 配置文件值
        private double _imageThreshold;
        private double _imageScaleRange;
        private int _collectPerLoop;      // 每轮采集次数（V+Shift+Z按几次）
        private int _maxLoops;             // 最大循环轮次

        public CollectResourceAction(ImageRecognition imageRec, HumanSimulator human, WindowHelper window, GameDelay delay)
            : base(imageRec, human, window, delay)
        {
            LoadConfig();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                // 读取图片识别阈值
                string thresholdStr = System.Configuration.ConfigurationManager.AppSettings["ImageThreshold"];
                _imageThreshold = string.IsNullOrEmpty(thresholdStr) ? 0.8 : double.Parse(thresholdStr);

                // 读取图片缩放范围
                string scaleStr = System.Configuration.ConfigurationManager.AppSettings["ImageScaleRange"];
                _imageScaleRange = string.IsNullOrEmpty(scaleStr) ? 0.1 : double.Parse(scaleStr);

                // 读取每轮采集次数（默认5次）
                string collectStr = System.Configuration.ConfigurationManager.AppSettings["CollectPerLoop"];
                _collectPerLoop = string.IsNullOrEmpty(collectStr) ? 5 : int.Parse(collectStr);

                // 读取最大循环轮次（默认3轮）
                string loopStr = System.Configuration.ConfigurationManager.AppSettings["MaxCollectLoops"];
                _maxLoops = string.IsNullOrEmpty(loopStr) ? 3 : int.Parse(loopStr);

                Log($"采集配置加载成功：阈值={_imageThreshold}，缩放={_imageScaleRange}，每轮采集={_collectPerLoop}次，最大轮次={_maxLoops}轮");
            }
            catch (Exception ex)
            {
                Log($"加载配置文件出错: {ex.Message}，使用默认值");
                _imageThreshold = 0.8;
                _imageScaleRange = 0.1;
                _collectPerLoop = 5;
                _maxLoops = 3;
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
            Log("=== 动作4: 采集资源 ===");

            try
            {
                // 第一步：按三下ESC清屏
                Log("按三下ESC清屏");
                for (int i = 0; i < 3; i++)
                {
                    _human.PressKey(Keys.Escape);
                    _delay.ActionDelay();
                }
                _delay.LongDelay();

                int collectedCount = 0;  // 总共采集次数
                int loopCount = 0;        // 当前轮次

                // 开始循环采集
                while (loopCount < _maxLoops)
                {
                    loopCount++;
                    Log($"=== 第 {loopCount} 轮采集 ===");

                    // 按小键盘1打开采集页面
                    Log("按小键盘1打开采集页面");
                    _human.PressKey(Keys.NumPad1);
                    _delay.LongDelay();

                    // 检查是否在采集页面
                    Log("检查是否在采集页面...");
                    bool inCollectPage = CheckImageExists(_collectPageImage, "采集页面");

                    if (!inCollectPage)
                    {
                        Log("❌ 无法打开采集页面，采集结束");
                        break;
                    }
                    Log("✅ 已在采集页面");

                    // 检查【资源放弃】按钮（存在则资源还在）
                    Log("检查【资源放弃】按钮...");
                    bool hasResource = CheckImageExists(_resourceCancelImage, "资源放弃");

                    if (!hasResource)
                    {
                        Log("✅ 【资源放弃】按钮不存在，资源已采完，采集结束");
                        break;
                    }
                    Log("✅ 【资源放弃】按钮存在，资源还在，开始采集");

                    // 按配置次数采集
                    for (int i = 0; i < _collectPerLoop; i++)
                    {
                        Log($"第 {i + 1} 次采集");

                        // 每次采集前按一下ESC
                        Log("按ESC准备采集");
                        _human.PressKey(Keys.Escape);
                        _delay.ActionDelay();

                        // 按 V+Shift+Z 组合键（先按V，再按Shift+Z）
                        Log("按 V 键");
                        _human.PressKey(Keys.V);                         // 先按 V
                        _delay.ActionDelay();

                        Log("按 Shift+Z 组合键");
                        _human.PressCombination(Keys.ShiftKey, Keys.Z);  // 再按 Shift+Z
                        _delay.ActionDelay(); ;// 等待采集动作
                        _delay.ActionDelay();// 等待采集动作
                        _delay.ActionDelay();// 等待采集动作
                        _delay.ActionDelay();// 等待采集动作
                        _delay.ActionDelay();// 等待采集动作
                        _delay.ActionDelay();// 等待采集动作

                        collectedCount++;
                        Log($"✅ 第 {i + 1} 次采集完成");
                    }

                    Log($"第 {loopCount} 轮采集完成，本轮采集 {_collectPerLoop} 次，累计 {collectedCount} 次");

                    // 等待一下再进入下一轮
                    _delay.LongDelay();
                }

                Log($"=== 采集完成，共采集 {collectedCount} 次 ===");
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ 执行出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查图片是否存在
        /// </summary>
        private bool CheckImageExists(string imageName, string description, int maxRetries = 1)
        {
            for (int retry = 0; retry < maxRetries; retry++)
            {
                if (retry > 0)
                {
                    Log($"第{retry + 1}次尝试检查{description}...");
                    _delay.ActionDelay();
                }

                Rectangle gameRect = new Rectangle(_gameX, _gameY, _windowWidth, _windowHeight);
                using (Bitmap gameScreenshot = _imageRec.CaptureRegion(gameRect))
                {
                    string templatesPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Templates"
                    );

                    string imagePath = Path.Combine(templatesPath, imageName + ".png");

                    if (!File.Exists(imagePath))
                    {
                        Log($"⚠️ 图片不存在: {imagePath}，无法检查");
                        return false;
                    }

                    using (Bitmap template = new Bitmap(imagePath))
                    {
                        Point? found = _imageRec.FindImage(template, gameScreenshot, _imageThreshold, _imageScaleRange);
                        if (found.HasValue)
                        {
                            Log($"✅ 找到{description}，位置: ({found.Value.X}, {found.Value.Y})");
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}