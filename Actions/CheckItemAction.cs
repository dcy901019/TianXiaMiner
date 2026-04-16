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
    /// 动作1：检查足通（固定坐标版）
    /// 在窗口指定区域查找足通图标，找不到就按小键盘2
    /// </summary>
    public class CheckItemAction : BaseAction
    {
        // 足通快捷键
        private Keys _shortcutKey = Keys.NumPad2;

        // 搜索区域（相对于窗口左上角）
        private int _searchX = 0;
        private int _searchY = 0;
        private int _searchWidth = 300;
        private int _searchHeight = 130;

        // 足通图标名称列表（在 Images/Templates/ 文件夹下）
        private string[] _zutongImages = new string[]
        {
            "金足通",
            "木足通",
            "宝足通",
            "神足通",
            "草足通",
            "低级神足通"
        };

        public CheckItemAction(ImageRecognition imageRec, HumanSimulator human, WindowHelper window, GameDelay delay)
            : base(imageRec, human, window, delay)
        {
        }

        /// <summary>
        /// 设置搜索区域（相对于窗口）
        /// </summary>
        public void SetSearchArea(int x, int y, int width, int height)
        {
            _searchX = x;
            _searchY = y;
            _searchWidth = width;
            _searchHeight = height;
        }

        /// <summary>
        /// 执行检查足通
        /// </summary>
        public override bool Execute()
        {
            Log("=== 检查足通 ===");

            try
            {
                // 计算实际屏幕坐标
                int screenX = _gameX + _searchX;
                int screenY = _gameY + _searchY;

                Log($"搜索区域: 窗口({_gameX},{_gameY}) + 偏移({_searchX},{_searchY}) = 屏幕({screenX},{screenY}) 大小{_searchWidth}x{_searchHeight}");

                // 截取搜索区域
                Rectangle searchRect = new Rectangle(screenX, screenY, _searchWidth, _searchHeight);

                // 确保截图区域在屏幕范围内
                Screen screen = Screen.PrimaryScreen;
                if (searchRect.X < 0) searchRect.X = 0;
                if (searchRect.Y < 0) searchRect.Y = 0;
                if (searchRect.Width > screen.Bounds.Width - searchRect.X)
                    searchRect.Width = screen.Bounds.Width - searchRect.X;
                if (searchRect.Height > screen.Bounds.Height - searchRect.Y)
                    searchRect.Height = screen.Bounds.Height - searchRect.Y;

                using (Bitmap screenshot = _imageRec.CaptureRegion(searchRect))
                {
                    // 获取模板文件夹路径
                    string templatesPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Templates"
                    );

                    bool foundAny = false;

                    // 遍历所有足通图标
                    foreach (string imageName in _zutongImages)
                    {
                        string imagePath = Path.Combine(templatesPath, imageName + ".png");

                        if (!File.Exists(imagePath))
                        {
                            Log($"⚠️ 图片不存在: {imageName}.png，跳过");
                            continue;
                        }

                        using (Bitmap template = new Bitmap(imagePath))
                        {
                            // 在截图中查找（阈值0.55，和按键精灵一致）
                            Point? found = _imageRec.FindImage(template, screenshot, 0.55, 0.2);

                            if (found.HasValue)
                            {
                                // 计算实际屏幕坐标
                                int actualX = screenX + found.Value.X;
                                int actualY = screenY + found.Value.Y;
                                Log($"✅ 找到{imageName}，位置({actualX},{actualY})");
                                foundAny = true;
                                break;
                            }
                        }
                    }

                    // 如果没有找到任何足通图标，按小键盘2使用足通
                    if (!foundAny)
                    {
                        Log("❌ 未找到任何足通图标，按小键盘2使用足通");
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