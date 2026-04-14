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
    /// 动作1：检查足通
    /// 在游戏窗口中查找足通图标
    /// 如果没有就按小键盘数字2
    /// </summary>
    public class CheckItemAction : BaseAction
    {
        // 足通图片名称前缀
        private string _imagePrefix = "足通状态图标";

        // 足通快捷键
        private Keys _shortcutKey = Keys.NumPad2;

        // 配置文件值
        private double _imageThreshold;
        private double _imageScaleRange;

        public CheckItemAction(ImageRecognition imageRec, HumanSimulator human, WindowHelper window, GameDelay delay)
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

                // 读取图片缩放范围（默认0.2）
                string scaleStr = System.Configuration.ConfigurationManager.AppSettings["ImageScaleRange"];
                if (!string.IsNullOrEmpty(scaleStr) && double.TryParse(scaleStr, out double scale))
                {
                    _imageScaleRange = scale;
                }
                else
                {
                    _imageScaleRange = 0.2;
                }

                Log($"配置文件加载成功：阈值={_imageThreshold}，缩放范围={_imageScaleRange}");
            }
            catch (Exception ex)
            {
                Log($"加载配置文件出错: {ex.Message}，使用默认值");
                _imageThreshold = 0.7;
                _imageScaleRange = 0.2;
            }
        }

        /// <summary>
        /// 设置足通图片前缀
        /// </summary>
        public void SetImagePrefix(string prefix)
        {
            _imagePrefix = prefix;
        }

        /// <summary>
        /// 设置足通快捷键
        /// </summary>
        public void SetShortcutKey(Keys key)
        {
            _shortcutKey = key;
        }

        /// <summary>
        /// 执行检查足通
        /// </summary>
        public override bool Execute()
        {
            Log("=== 检查足通 ===");

            try
            {
                // 1. 显示当前窗口信息
                Log($"当前窗口位置: ({_gameX}, {_gameY}) 大小: {_windowWidth}x{_windowHeight}");

                // 检查窗口坐标是否有效
                if (_windowWidth <= 0 || _windowHeight <= 0)
                {
                    Log($"❌ 窗口大小无效: {_windowWidth}x{_windowHeight}");
                    return false;
                }

                // 检查屏幕边界
                Screen screen = Screen.PrimaryScreen;
                Log($"屏幕大小: {screen.Bounds.Width}x{screen.Bounds.Height}");

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
                    // 2. 保存调试截图
                    string debugPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Screenshots",
                        $"game_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                    );
                    gameScreenshot.Save(debugPath);
                    Log($"已保存游戏窗口截图: {debugPath}");

                    // 3. 获取所有足通图标模板
                    string templatesPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Templates"
                    );

                    Log($"模板路径: {templatesPath}");

                    // 检查目录是否存在
                    if (!Directory.Exists(templatesPath))
                    {
                        Log($"❌ 模板目录不存在: {templatesPath}");
                        return false;
                    }

                    string[] imageFiles = Directory.GetFiles(templatesPath, $"{_imagePrefix}*.png");
                    Log($"找到 {imageFiles.Length} 个足通图标模板");

                    // 列出所有找到的文件
                    foreach (string file in imageFiles)
                    {
                        Log($"  文件: {Path.GetFileName(file)}");
                    }

                    bool foundAny = false;

                    // 4. 逐个匹配图片
                    foreach (string imageFile in imageFiles)
                    {
                        string imageName = Path.GetFileNameWithoutExtension(imageFile);
                        Log($"正在匹配: {imageName}");

                        try
                        {
                            // 每次循环都重新加载图片，避免共享资源
                            using (Bitmap template = new Bitmap(imageFile))
                            {
                                Log($"模板图片大小: {template.Width}x{template.Height}");

                                // 每次匹配都使用新的截图
                                using (Bitmap freshScreenshot = _imageRec.CaptureRegion(gameRect))
                                {
                                    Point? found = _imageRec.FindImage(template, freshScreenshot, _imageThreshold, _imageScaleRange);

                                    if (found.HasValue)
                                    {
                                        int screenX = _gameX + found.Value.X;
                                        int screenY = _gameY + found.Value.Y;

                                        Log($"✅ 找到足通图标: {imageName} 位置({screenX},{screenY})");
                                        foundAny = true;
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"❌ 处理图片 {imageName} 时出错: {ex.Message}");
                        }
                    }

                    // 5. 如果没有找到任何足通图标，按快捷键使用足通
                    if (!foundAny)
                    {
                        Log("❌ 未找到任何足通图标，按小键盘数字2使用足通");
                        _human.PressKey(_shortcutKey);
                        _delay.ActionDelay();
                        Log("✅ 已使用足通");
                    }
                    else
                    {
                        Log("足通已存在，无需操作");
                    }
                }

                Log("=== 检测足通完成 ===");
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ 执行出错: {ex.Message}");
                return false;
            }
        }
    }
}