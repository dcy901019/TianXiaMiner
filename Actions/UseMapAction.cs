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
    /// 动作3：地图找资源
    /// 1. 小地图找资源：遍历7个地图查找资源图标
    /// 2. 大地图寻路：打开大地图，定位到找到资源的地图，点击资源图标并确认前往
    /// </summary>
    public class UseMapAction : BaseAction
    {
        // 采集地图页面图片
        private string _collectionMapPage = "采集地图页面";

        // 小地图名称列表（带"采集"前缀）- 现在包含7个地图
        private string[] _minimapNames = new string[]
        {
            "采集地图_天虞岛",
            "采集地图_九黎",
            "采集地图_巴蜀",
            "采集地图_中原",
            "采集地图_江南",
             "采集地图_雷泽" ,
            "采集地图_燕丘"
        };

        // 大地图名称列表（不带"采集"前缀）- 现在包含7个地图
        private string[] _worldmapNames = new string[]
        {
            "地图_天虞岛",
            "地图_九黎",
            "地图_巴蜀",
            "地图_中原",
            "地图_江南",
            "地图_雷泽",
            "地图_燕丘"
        };

        // 小地图资源图标前缀（带"采集"前缀）
        private string _minimapResourcePrefix = "采集地图_资源图标";

        // 大地图资源图标前缀（不带"采集"前缀）
        private string _worldmapResourcePrefix = "资源图标";

        // 全地图按钮图片
        private string _fullMapButton = "全地图按钮";

        // 确认前往按钮图片
        private string _confirmButton = "确定";

        // 记录找到资源的地图索引
        private int _foundMapIndex = -1;

        // 配置文件值
        private double _imageThreshold;
        private double _imageScaleRange;

        public UseMapAction(ImageRecognition imageRec, HumanSimulator human, WindowHelper window, GameDelay delay)
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
        public bool Execute(FindAllWindowsAction.GameWindowInfo window)
        {
            // 设置窗口坐标
            SetWindowPosition(window.X, window.Y, window.Width, window.Height);

            // 调用实际的执行逻辑
            return ExecuteInternal();
        }

        /// <summary>
        /// 实际的执行逻辑
        /// </summary>
        private bool ExecuteInternal()
        {
            Log("=== 动作3: 地图找资源 ===");

            try
            {
                // ========== 第一步：检查采集地图页面 ==========
                Log("检查是否在采集地图页面...");
                bool inCollectionPage = CheckImageExists(_collectionMapPage, "采集地图页面");

                if (!inCollectionPage)
                {
                    Log("❌ 不在采集地图页面，按小键盘数字1打开");
                    _human.PressKey(Keys.NumPad1);
                    _delay.LongDelay();

                    inCollectionPage = CheckImageExists(_collectionMapPage, "采集地图页面", 2);
                    if (!inCollectionPage)
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

                _delay.ActionDelay();

                // ========== 第二步：小地图找资源 ==========
                Log("=== 开始小地图找资源 ===");
                bool resourceFound = FindResourceOnMinimap();

                if (!resourceFound || _foundMapIndex == -1)
                {
                    Log("❌ 所有地图都未找到资源图标");
                    return false;
                }

                Log($"✅ 在 {_minimapNames[_foundMapIndex]} 找到资源图标");

                // ========== 第三步：执行动作1 检查足通 ==========
                Log("=== 执行动作1: 检查足通 ===");
                bool checkItemResult = CheckItem();

                if (!checkItemResult)
                {
                    Log("⚠️ 检查足通过程出现异常，但继续执行大地图寻路");
                }

                // ========== 第四步：大地图寻路 ==========
                Log("=== 开始大地图寻路 ===");

                // 按小键盘数字M打开大地图
                Log("按小键盘数字M打开大地图");
                _human.PressKey(Keys.M);
                _delay.LongDelay();

                // 使用【大地图定位】图片来定位
                if (!LocateAndClickOnWorldmap())
                {
                    Log("❌ 无法在大地图中定位");
                    return false;
                }

                // 等待地图加载
                _delay.LongDelay();

                // 在大地图中查找资源图标
                if (!FindAndClickResourceOnWorldmap())
                {
                    Log("❌ 在大地图中找不到资源图标");
                    return false;
                }

                // 按下回车键确认前往
                Log("按下回车键确认前往");
                _human.PressKey(Keys.Enter);
                _delay.LongDelay();

                Log("=== 动作3完成 ===");
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ 执行出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 执行动作1：检查足通
        /// </summary>
        private bool CheckItem()
        {
            Log("开始检查足通状态...");

            try
            {
                // 创建检查足通动作
                CheckItemAction checkItem = new CheckItemAction(_imageRec, _human, _window, _delay);

                // 设置窗口坐标
                checkItem.SetWindowPosition(_gameX, _gameY, _windowWidth, _windowHeight);

                // 订阅日志（将日志转发到当前动作的日志）
                checkItem.OnLog += (logMessage) => Log(logMessage);

                // 执行检查足通
                bool result = checkItem.Execute();

                Log(result ? "✅ 足通检查完成" : "⚠️ 足通检查返回失败");
                return result;
            }
            catch (Exception ex)
            {
                Log($"❌ 检查足通时出错: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// 使用【大地图定位】图片来定位并点击地图
        /// </summary>
        private bool LocateAndClickOnWorldmap()
        {
            Log("使用大地图定位图片...");

            try
            {
                string templatesPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images",
                    "Templates"
                );

                string locateImagePath = Path.Combine(templatesPath, "大地图定位.png");
                if (!File.Exists(locateImagePath))
                {
                    Log($"❌ 大地图定位图片不存在: {locateImagePath}");
                    return false;
                }

                // 截取游戏窗口
                Rectangle gameRect = new Rectangle(_gameX, _gameY, _windowWidth, _windowHeight);
                using (Bitmap gameScreenshot = _imageRec.CaptureRegion(gameRect))
                using (Bitmap locateTemplate = new Bitmap(locateImagePath))
                {
                    // 查找大地图定位图片
                    Point? locateFound = _imageRec.FindImage(locateTemplate, gameScreenshot, _imageThreshold, _imageScaleRange);

                    if (!locateFound.HasValue)
                    {
                        Log("❌ 未找到大地图定位图片");
                        return false;
                    }

                    // 获取找到的图片的左上角坐标和尺寸
                    int foundX = locateFound.Value.X;
                    int foundY = locateFound.Value.Y;
                    int foundWidth = locateTemplate.Width;
                    int foundHeight = locateTemplate.Height;

                    Log($"找到大地图定位图片，左上角: ({foundX}, {foundY})");
                    Log($"图片尺寸: {foundWidth}x{foundHeight}");

                    // 计算下方点击位置（X坐标不变，Y坐标向下偏移图片高度）
                    int downClickX = foundX + foundWidth / 2;  // 点击图片中心X
                    int downClickY = foundY + foundHeight;      // 点击图片正下方（Y坐标 + 图片高度）

                    Log($"下方点击位置: ({downClickX}, {downClickY})");

                    // 在定位图片下方右键点击3次
                    Log($"准备在定位图片下方右键点击3次...");

                    // 第一次右键点击
                    _human.MoveMouseLikeHuman(new Point(downClickX, downClickY), _gameX, _gameY);
                    _delay.ActionDelay();
                    _human.RightClick();
                    _delay.ActionDelay();
                    Log($"✅ 第1次下方右键点击");

                    // 第二次右键点击
                    _human.RightClick();
                    _delay.ActionDelay();
                    Log($"✅ 第2次下方右键点击");

                    // 第三次右键点击
                    _human.RightClick();
                    _delay.LongDelay(); // 等待区域展开
                    Log($"✅ 第3次下方右键点击");

                    // 现在查找具体的地图名称
                    string targetMapName = _worldmapNames[_foundMapIndex];
                    Log($"要查找的大地图名称: {targetMapName}");

                    string mapImagePath = Path.Combine(templatesPath, targetMapName + ".png");
                    if (!File.Exists(mapImagePath))
                    {
                        Log($"❌ 地图名称图片不存在: {mapImagePath}");
                        return false;
                    }

                    using (Bitmap mapTemplate = new Bitmap(mapImagePath))
                    {
                        // 重新截屏（确保是最新画面）
                        using (Bitmap newScreenshot = _imageRec.CaptureRegion(gameRect))
                        {
                            // 查找地图名称
                            Point? mapFound = _imageRec.FindImage(mapTemplate, newScreenshot, _imageThreshold, _imageScaleRange);

                            if (!mapFound.HasValue)
                            {
                                Log($"❌ 未找到地图名称: {targetMapName}");
                                return false;
                            }

                            // 计算点击位置（图片中心点）
                            int clickX = mapFound.Value.X + mapTemplate.Width / 2;
                            int clickY = mapFound.Value.Y + mapTemplate.Height / 2;

                            Log($"找到地图名称，左上角: ({mapFound.Value.X}, {mapFound.Value.Y})");
                            Log($"地图名称尺寸: {mapTemplate.Width}x{mapTemplate.Height}");
                            Log($"点击位置: ({clickX}, {clickY})");

                            // 点击地图名称
                            _human.MoveMouseLikeHuman(new Point(clickX, clickY), _gameX, _gameY);
                            _delay.ActionDelay();
                            _human.LeftClick();
                            _delay.LongDelay();

                            Log($"✅ 点击地图名称: {targetMapName}");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"大地图定位时出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 小地图找资源 - 遍历所有地图查找资源图标
        /// </summary>
        private bool FindResourceOnMinimap()
        {
            // 截取游戏窗口
            Rectangle gameRect = new Rectangle(_gameX, _gameY, _windowWidth, _windowHeight);

            string templatesPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                "Templates"
            );

            // 获取所有小地图资源图标图片
            string[] minimapResourceFiles = Directory.GetFiles(templatesPath, $"{_minimapResourcePrefix}*.png");

            if (minimapResourceFiles.Length == 0)
            {
                Log($"⚠️ 没有找到任何小地图资源图标图片（应以'{_minimapResourcePrefix}'开头）");
                return false;
            }

            Log($"找到 {minimapResourceFiles.Length} 个小地图资源图标");

            // 遍历每个小地图
            for (int i = 0; i < _minimapNames.Length; i++)
            {
                string mapName = _minimapNames[i];
                Log($"尝试点击地图: {mapName}");

                string mapPath = Path.Combine(templatesPath, mapName + ".png");
                if (!File.Exists(mapPath))
                {
                    Log($"⚠️ 地图图片不存在: {mapPath}，跳过");
                    continue;
                }

                try
                {
                    // 重新截屏（每次点击前都重新截屏，确保是最新画面）
                    using (Bitmap gameScreenshot = _imageRec.CaptureRegion(gameRect))
                    {
                        // 查找地图
                        using (Bitmap mapTemplate = new Bitmap(mapPath))
                        {
                            // 使用配置文件中的阈值和缩放范围
                            Point? mapFound = _imageRec.FindImage(mapTemplate, gameScreenshot, _imageThreshold, _imageScaleRange);

                            if (mapFound.HasValue)
                            {
                                // 点击地图
                                _human.MoveMouseLikeHuman(new Point(mapFound.Value.X, mapFound.Value.Y), _gameX, _gameY);
                                _delay.ActionDelay();
                                _human.LeftClick();
                                _delay.LongDelay(); // 等待地图加载

                                Log($"✅ 已点击 {mapName}");

                                // 重新截屏找资源图标
                                using (Bitmap newScreenshot = _imageRec.CaptureRegion(gameRect))
                                {
                                    // 在当前地图上找资源图标
                                    bool foundInThisMap = false;

                                    foreach (string resourceFile in minimapResourceFiles)
                                    {
                                        try
                                        {
                                            string resourceName = Path.GetFileNameWithoutExtension(resourceFile);
                                            Log($"尝试查找小地图资源图标: {resourceName}");

                                            using (Bitmap resourceTemplate = new Bitmap(resourceFile))
                                            {
                                                // 使用配置文件中的阈值和缩放范围
                                                Point? resourceFound = _imageRec.FindImage(resourceTemplate, newScreenshot, _imageThreshold, _imageScaleRange);

                                                if (resourceFound.HasValue)
                                                {
                                                    _foundMapIndex = i; // 记录找到资源的地图索引
                                                    Log($"✅ 在 {mapName} 上找到 {resourceName}");
                                                    return true; // 找到资源，返回成功
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Log($"查找小地图资源图标时出错: {ex.Message}，继续下一个");
                                        }
                                    }

                                    // 如果这个地图没找到资源，继续尝试下一个地图
                                    Log($"❌ {mapName} 上没有找到任何小地图资源图标，继续下一个地图");
                                    // 注意：这里没有return，会继续循环
                                }
                            }
                            else
                            {
                                Log($"❌ 未找到 {mapName}，可能是地图已点击过或不在视野内，继续下一个地图");
                                // 继续下一个地图
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"处理地图 {mapName} 时出错: {ex.Message}，继续下一个地图");
                    // 继续下一个地图
                }
            }

            Log("❌ 所有地图都遍历完毕，未找到任何资源图标");
            return false;
        }

        /// <summary>
        /// 点击全地图按钮（3次确保大地图是原始状态）
        /// </summary>
        private bool ClickFullMapButton()
        {
            Log("尝试点击全地图按钮...");

            string templatesPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images",
                "Templates"
            );

            string fullMapPath = Path.Combine(templatesPath, _fullMapButton + ".png");
            if (!File.Exists(fullMapPath))
            {
                Log($"⚠️ 全地图按钮图片不存在: {fullMapPath}，跳过");
                return true;
            }

            // 尝试点击3次
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Rectangle gameRect = new Rectangle(_gameX, _gameY, _windowWidth, _windowHeight);
                    using (Bitmap gameScreenshot = _imageRec.CaptureRegion(gameRect))
                    using (Bitmap template = new Bitmap(fullMapPath))
                    {
                        // 使用配置文件中的阈值和缩放范围
                        Point? found = _imageRec.FindImage(template, gameScreenshot, _imageThreshold, _imageScaleRange);

                        if (found.HasValue)
                        {
                            _human.MoveMouseLikeHuman(new Point(found.Value.X, found.Value.Y), _gameX, _gameY);
                            _delay.ActionDelay();
                            _human.LeftClick();
                            _delay.ActionDelay();

                            Log($"✅ 第{i + 1}次点击全地图按钮");
                        }
                        else
                        {
                            if (i == 0)
                            {
                                Log("❌ 未找到全地图按钮");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"点击全地图按钮时出错: {ex.Message}");
                }
            }

            return true;
        }

        /// <summary>
        /// 在大地图中点击指定的地图
        /// </summary>
        private bool ClickMapOnWorldmap(string mapName)
        {
            Log($"在大地图中查找: {mapName}");

            try
            {
                Rectangle gameRect = new Rectangle(_gameX, _gameY, _windowWidth, _windowHeight);
                using (Bitmap gameScreenshot = _imageRec.CaptureRegion(gameRect))
                {
                    string templatesPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Templates"
                    );

                    string mapPath = Path.Combine(templatesPath, mapName + ".png");
                    if (!File.Exists(mapPath))
                    {
                        Log($"⚠️ 地图图片不存在: {mapPath}");
                        return false;
                    }

                    using (Bitmap template = new Bitmap(mapPath))
                    {
                        // 使用配置文件中的阈值和缩放范围
                        Point? found = _imageRec.FindImage(template, gameScreenshot, _imageThreshold, _imageScaleRange);

                        if (found.HasValue)
                        {
                            _human.MoveMouseLikeHuman(new Point(found.Value.X, found.Value.Y), _gameX, _gameY);
                            _delay.ActionDelay();
                            _human.LeftClick();
                            _delay.LongDelay(); // 等待地图加载

                            Log($"✅ 在大地图中点击 {mapName}");
                            return true;
                        }
                        else
                        {
                            Log($"❌ 在大地图中未找到 {mapName}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"点击大地图 {mapName} 时出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 在大地图中查找并点击资源图标
        /// </summary>
        private bool FindAndClickResourceOnWorldmap()
        {
            Log("在大地图中查找资源图标...");

            try
            {
                // 读取小图标专用配置
                double smallThreshold = 0.55;
                double smallScaleRange = 0.25;

                try
                {
                    string thresholdStr = System.Configuration.ConfigurationManager.AppSettings["SmallImageThreshold"];
                    if (!string.IsNullOrEmpty(thresholdStr) && double.TryParse(thresholdStr, out double t))
                    {
                        smallThreshold = t;
                    }

                    string scaleStr = System.Configuration.ConfigurationManager.AppSettings["SmallImageScaleRange"];
                    if (!string.IsNullOrEmpty(scaleStr) && double.TryParse(scaleStr, out double s))
                    {
                        smallScaleRange = s;
                    }

                    Log($"小图标配置: 阈值={smallThreshold}, 缩放范围={smallScaleRange}");
                }
                catch (Exception ex)
                {
                    Log($"读取小图标配置出错: {ex.Message}，使用默认值");
                }

                Rectangle gameRect = new Rectangle(_gameX, _gameY, _windowWidth, _windowHeight);
                using (Bitmap gameScreenshot = _imageRec.CaptureRegion(gameRect))
                {
                    string templatesPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Templates"
                    );

                    // 获取所有大地图资源图标图片
                    string[] resourceFiles = Directory.GetFiles(templatesPath, $"{_worldmapResourcePrefix}*.png");

                    if (resourceFiles.Length == 0)
                    {
                        Log($"⚠️ 没有找到以 '{_worldmapResourcePrefix}' 开头的资源图标图片");
                        return false;
                    }

                    Log($"找到 {resourceFiles.Length} 个大地图资源图标");

                    foreach (string resourceFile in resourceFiles)
                    {
                        try
                        {
                            string resourceName = Path.GetFileNameWithoutExtension(resourceFile);
                            Log($"尝试查找大地图资源图标: {resourceName}");

                            using (Bitmap template = new Bitmap(resourceFile))
                            {
                                Log($"模板图片大小: {template.Width}x{template.Height}");

                                // 使用小图标专用配置（较低的阈值，较大的缩放范围）
                                Point? found = _imageRec.FindImage(template, gameScreenshot, smallThreshold, smallScaleRange);

                                if (found.HasValue)
                                {
                                    int screenX = _gameX + found.Value.X;
                                    int screenY = _gameY + found.Value.Y;

                                    Log($"✅ 找到大地图资源图标: {resourceName} 位置({screenX},{screenY})");

                                    // 移动鼠标并左键点击
                                    _human.MoveMouseLikeHuman(new Point(found.Value.X, found.Value.Y), _gameX, _gameY);
                                    _delay.ActionDelay();
                                    _human.LeftClick();
                                    _delay.LongDelay();

                                    Log($"✅ 已点击资源图标: {resourceName}");
                                    return true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"查找大地图资源图标时出错: {ex.Message}，继续下一个");
                        }
                    }

                    Log("❌ 在大地图中未找到任何资源图标");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log($"查找大地图资源图标时出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 点击确认前往按钮
        /// </summary>
        private bool ClickConfirmButton()
        {
            Log("查找确认前往按钮...");

            try
            {
                Rectangle gameRect = new Rectangle(_gameX, _gameY, _windowWidth, _windowHeight);
                using (Bitmap gameScreenshot = _imageRec.CaptureRegion(gameRect))
                {
                    string templatesPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Templates"
                    );

                    string confirmPath = Path.Combine(templatesPath, _confirmButton + ".png");
                    if (!File.Exists(confirmPath))
                    {
                        Log($"⚠️ 确认按钮图片不存在: {confirmPath}，跳过");
                        return true;
                    }

                    using (Bitmap template = new Bitmap(confirmPath))
                    {
                        // 使用配置文件中的阈值和缩放范围
                        Point? found = _imageRec.FindImage(template, gameScreenshot, _imageThreshold, _imageScaleRange);

                        if (found.HasValue)
                        {
                            _human.MoveMouseLikeHuman(new Point(found.Value.X, found.Value.Y), _gameX, _gameY);
                            _delay.ActionDelay();
                            _human.LeftClick();
                            _delay.ActionDelay();

                            Log("✅ 点击确认前往按钮");
                            return true;
                        }
                        else
                        {
                            Log("❌ 未找到确认前往按钮");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"点击确认按钮时出错: {ex.Message}");
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

                try
                {
                    Rectangle gameRect = new Rectangle(_gameX, _gameY, _windowWidth, _windowHeight);
                    using (Bitmap gameScreenshot = _imageRec.CaptureRegion(gameRect))
                    {
                        // 保存调试截图
                        string debugPath = Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            "Images",
                            "Screenshots",
                            $"check_{imageName}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
                        );
                        gameScreenshot.Save(debugPath);
                        Log($"已保存检查截图: {debugPath}");

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
                catch (Exception ex)
                {
                    Log($"检查{description}时出错: {ex.Message}");
                }
            }
            return false;
        }
    }
}