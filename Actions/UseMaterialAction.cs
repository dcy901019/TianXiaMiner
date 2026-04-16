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
    /// 动作2：放入感应材料
    /// 1. 判断是否在采集地图页面，不在则按数字1打开
    /// 2. 判断包裹是否打开，没开则按数字5打开
    /// 3. 在包裹中找材料并右键点击
    /// 4. 关闭包裹
    /// </summary>
    public class UseMaterialAction : BaseAction
    {
        // 放入页面图片名称
        private string _putPageImage = "采集地图页面";

        // 包裹图片名称（用于判断包裹是否打开）
        private string _bagImage = "包裹界面";

        // 材料图片前缀
        private string _materialPrefix = "感应材料";

        // 配置文件值
        private double _imageThreshold;
        private double _imageScaleRange;
        private int _clickCount;
        private double _offsetMultiplier;

        public UseMaterialAction(ImageRecognition imageRec, HumanSimulator human, WindowHelper window, GameDelay delay)
            : base(imageRec, human, window, delay)
        {
            // 初始化时加载配置
            LoadConfig();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                // 读取图片识别阈值（默认0.7）
                string thresholdStr = System.Configuration.ConfigurationManager.AppSettings["ImageThreshold"];
                if (!string.IsNullOrEmpty(thresholdStr) && double.TryParse(thresholdStr, out double threshold))
                {
                    _imageThreshold = threshold;
                }
                else
                {
                    _imageThreshold = 0.7;
                }

                // 读取图片缩放范围（默认0.1）
                string scaleStr = System.Configuration.ConfigurationManager.AppSettings["ImageScaleRange"];
                if (!string.IsNullOrEmpty(scaleStr) && double.TryParse(scaleStr, out double scale))
                {
                    _imageScaleRange = scale;
                }
                else
                {
                    _imageScaleRange = 0.1;
                }

                // 读取点击格子数量（默认3）
                string countStr = System.Configuration.ConfigurationManager.AppSettings["MaterialClickCount"];
                if (!string.IsNullOrEmpty(countStr) && int.TryParse(countStr, out int count))
                {
                    _clickCount = count;
                }
                else
                {
                    _clickCount = 3;
                }

                // 读取偏移倍率（默认1.2）
                string offsetStr = System.Configuration.ConfigurationManager.AppSettings["OffsetMultiplier"];
                if (!string.IsNullOrEmpty(offsetStr) && double.TryParse(offsetStr, out double offset))
                {
                    _offsetMultiplier = offset;
                }
                else
                {
                    _offsetMultiplier = 1.2;
                }

                Log($"配置文件加载成功：阈值={_imageThreshold}，缩放范围={_imageScaleRange}，点击数量={_clickCount}，偏移倍率={_offsetMultiplier}");
            }
            catch (Exception ex)
            {
                Log($"加载配置文件出错: {ex.Message}，使用默认值");
                _imageThreshold = 0.7;
                _imageScaleRange = 0.1;
                _clickCount = 3;
                _offsetMultiplier = 1.2;
            }
        }

        /// <summary>
        /// 设置放入页面图片名称
        /// </summary>
        public void SetPutPageImage(string imageName)
        {
            _putPageImage = imageName;
        }

        /// <summary>
        /// 设置包裹图片名称
        /// </summary>
        public void SetBagImage(string imageName)
        {
            _bagImage = imageName;
        }

        /// <summary>
        /// 设置材料图片前缀
        /// </summary>
        public void SetMaterialPrefix(string prefix)
        {
            _materialPrefix = prefix;
        }

        /// <summary>
        /// 必须实现的抽象方法（无参数）- 不应该直接调用
        /// </summary>
        public override bool Execute()
        {
            Log("错误：请使用带窗口参数的 Execute 方法");
            return false;
        }

        /// <summary>
        /// 执行动作（带窗口参数）- 实际使用的方法
        /// </summary>
        public bool ExecuteWithWindow(FindAllWindowsAction.GameWindowInfo window)
        {
            Log($"ExecuteWithWindow 方法被调用，窗口位置: ({window.X}, {window.Y}) 大小 {window.Width}x{window.Height}");

            // 设置窗口坐标
            SetWindowPosition(window.X, window.Y, window.Width, window.Height);

            Log($"窗口坐标已设置: ({_gameX}, {_gameY}) 大小 {_windowWidth}x{_windowHeight}");

            // 调用实际的执行逻辑
            return ExecuteInternal();
        }

        /// <summary>
        /// 实际的执行逻辑
        /// </summary>
        private bool ExecuteInternal()
        {
            Log("=== 动作2: 放入感应材料 ===");
            Log($"当前窗口位置: ({_gameX}, {_gameY}) 大小 {_windowWidth}x{_windowHeight}");

            try
            {
                // ========== 第一步：检查采集地图页面 ==========
                Log("检查是否在采集地图页面...");
                bool inPutPage = CheckImageExists(_putPageImage, "采集地图页面");

                if (!inPutPage)
                {
                    Log("❌ 不在采集地图页面，按小键盘数字1打开");
                    _human.PressKey(Keys.NumPad1);
                    _delay.LongDelay();

                    // 再次确认是否打开成功
                    inPutPage = CheckImageExists(_putPageImage, "采集地图页面", 2);
                    if (!inPutPage)
                    {
                        Log("❌ 无法打开采集地图页面");
                        return false;
                    }
                    Log("✅ 已打开采集地图页面");
                }
                else
                {
                    Log("✅ 已在采集地图页面");
                }

                // ========== 第二步：检查包裹是否打开 ==========
                Log("检查包裹是否打开...");
                bool bagOpen = CheckImageExists(_bagImage, "包裹界面");

                // 如果没打开，尝试打开（最多3次）
                int maxAttempts = 3;
                int attempt = 1;

                while (!bagOpen && attempt <= maxAttempts)
                {
                    Log($"❌ 包裹未打开，按小键盘数字5打开包裹 (第{attempt}次)");
                    _human.PressKey(Keys.B);
                    _delay.LongDelay();

                    bagOpen = CheckImageExists(_bagImage, "包裹界面", 2);
                    attempt++;
                }

                if (!bagOpen)
                {
                    Log("❌ 无法打开包裹");
                    return false;
                }

                Log("✅ 包裹已打开");

                // ========== 第三步：在包裹中找材料 ==========
                Log("开始在包裹中查找感应材料...");
                bool materialFound = FindMaterialInBag();

                if (!materialFound)
                {
                    Log("⚠️ 未找到任何感应材料，继续执行后续步骤");
                }
                else
                {
                    Log("✅ 已找到并点击材料");
                }

                // ========== 第四步：关闭包裹 ==========
                Log("关闭包裹（B）");
                _human.PressKey(Keys.B);
                _delay.ActionDelay();

                Log("=== 动作2完成 ===");
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ 执行出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查图片是否存在（带重试）
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

                // 截取游戏窗口
                Rectangle gameRect = new Rectangle(_gameX, _gameY, _windowWidth, _windowHeight);
                using (Bitmap gameScreenshot = _imageRec.CaptureRegion(gameRect))
                {
                    string templatesPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Templates"
                    );

                    string imagePath = Path.Combine(templatesPath, imageName + ".png");

                    // 检查图片文件是否存在
                    if (!File.Exists(imagePath))
                    {
                        Log($"⚠️ 图片不存在: {imagePath}，无法检查");
                        return false;
                    }

                    using (Bitmap template = new Bitmap(imagePath))
                    {
                        // 使用配置文件中的阈值和缩放范围
                        Point? found = _imageRec.FindImage(template, gameScreenshot, _imageThreshold, _imageScaleRange);
                        if (found.HasValue)
                        {
                            Log($"✅ 找到{description}，位置: ({found.Value.X}, {found.Value.Y})");
                            return true;
                        }
                        else
                        {
                            Log($"❌ 未找到{description}");
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 在包裹中查找材料 - 基于找到图片的偏移模式
        /// </summary>
        private bool FindMaterialInBag()
        {
            Log("开始 FindMaterialInBag 方法（基于找到图片的偏移模式）");

            try
            {
                // 等待包裹完全加载
                Log("等待包裹完全加载...");
                _delay.LongDelay();

                // 检查窗口坐标是否有效
                if (_windowWidth <= 0 || _windowHeight <= 0)
                {
                    Log($"❌ 窗口大小无效: {_windowWidth}x{_windowHeight}");
                    return false;
                }

                // 检查屏幕边界
                Screen screen = Screen.PrimaryScreen;

                // 确保截图区域在屏幕范围内
                Rectangle gameRect = new Rectangle(
                    Math.Max(0, _gameX),
                    Math.Max(0, _gameY),
                    Math.Min(_windowWidth, screen.Bounds.Width - _gameX),
                    Math.Min(_windowHeight, screen.Bounds.Height - _gameY)
                );

                if (gameRect.Width <= 0 || gameRect.Height <= 0)
                {
                    Log($"❌ 截图区域无效: {gameRect}");
                    return false;
                }

                Log($"截取游戏窗口区域: ({gameRect.X}, {gameRect.Y}) {gameRect.Width}x{gameRect.Height}");

                using (Bitmap gameScreenshot = _imageRec.CaptureRegion(gameRect))
                {
                    if (gameScreenshot == null)
                    {
                        Log("❌ 截图失败，返回空");
                        return false;
                    }


                    // 查找包裹界面图片
                    string templatesPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Templates"
                    );

                    string bagImagePath = Path.Combine(templatesPath, _bagImage + ".png");

                    if (!File.Exists(bagImagePath))
                    {
                        Log($"❌ 包裹界面图片不存在: {bagImagePath}");
                        return false;
                    }

                    using (Bitmap bagTemplate = new Bitmap(bagImagePath))
                    {
                        Log($"包裹界面模板大小: {bagTemplate.Width}x{bagTemplate.Height}");

                        // 在截图中查找包裹界面，使用配置文件中的阈值
                        Point? bagFound = _imageRec.FindImage(bagTemplate, gameScreenshot, _imageThreshold, _imageScaleRange);

                        if (!bagFound.HasValue)
                        {
                            Log("❌ 未找到包裹界面图片");
                            return false;
                        }

                        // 获取找到的图片的左上角坐标和尺寸
                        int foundX = bagFound.Value.X;
                        int foundY = bagFound.Value.Y;
                        int foundWidth = bagTemplate.Width;
                        int foundHeight = bagTemplate.Height;

                        Log($"找到图片左上角: ({foundX}, {foundY})");
                        Log($"图片尺寸: {foundWidth}x{foundHeight}");

                        // 计算偏移量（图片宽度的配置倍率）
                        int offset = (int)(foundWidth * _offsetMultiplier);
                        Log($"偏移量: {foundWidth} × {_offsetMultiplier} = {offset}");

                        // 使用配置文件中的点击数量
                        Log($"配置文件设置: 点击 {_clickCount} 个格子");

                        // 循环点击指定数量的格子
                        for (int i = 1; i <= _clickCount; i++)
                        {
                            // 计算点击位置：
                            // X坐标 = 找到图片的X坐标 + (偏移量 × i)
                            // Y坐标 = 找到图片的Y坐标（直接用图片的Y坐标）
                            int clickX = foundX + (offset * i);
                            int clickY = foundY;

                            Log($"第{i}个格子位置: ({clickX}, {clickY})");

                            // 移动鼠标并右键点击
                            _human.MoveMouseLikeHuman(new Point(clickX, clickY), _gameX, _gameY);
                            _delay.ActionDelay();
                            _human.RightClick();
                            _delay.ActionDelay();

                            Log($"✅ 已点击第{i}个格子");
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"FindMaterialInBag 整体出错: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// 自然排序比较器（让文件名按数字顺序排序）
    /// </summary>
    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return NativeStringComparer.CompareStrings(x, y);
        }
    }

    /// <summary>
    /// Windows原生字符串比较（支持数字自然排序）
    /// </summary>
    internal static class NativeStringComparer
    {
        [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        public static int CompareStrings(string s1, string s2)
        {
            return StrCmpLogicalW(s1, s2);
        }
    }
}