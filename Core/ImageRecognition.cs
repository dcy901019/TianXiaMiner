using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = System.Drawing.Point;

namespace TianXiaMiner.Core
{
    /// <summary>
    /// 图片识别类 - 使用OpenCvSharp4
    /// </summary>
    public class ImageRecognition
    {
        // 截图配置
        private static bool _saveDebugScreenshots = false;
        private static int _maxScreenshotsToKeep = 50;
        private static string _screenshotFolder = null;
        private static bool _configLoaded = false;

        /// <summary>
        /// 加载配置文件（只加载一次）
        /// </summary>
        private static void LoadConfig()
        {
            if (_configLoaded) return;

            try
            {
                string saveConfig = System.Configuration.ConfigurationManager.AppSettings["SaveDebugScreenshots"];
                if (!string.IsNullOrEmpty(saveConfig))
                {
                    _saveDebugScreenshots = saveConfig.ToLower() == "true";
                }

                string maxConfig = System.Configuration.ConfigurationManager.AppSettings["MaxScreenshotsToKeep"];
                if (!string.IsNullOrEmpty(maxConfig) && int.TryParse(maxConfig, out int max))
                {
                    _maxScreenshotsToKeep = max;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载截图配置失败: {ex.Message}");
            }

            _configLoaded = true;
            System.Diagnostics.Debug.WriteLine($"截图配置: 保存={_saveDebugScreenshots}, 保留数量={_maxScreenshotsToKeep}");
        }

        /// <summary>
        /// 清理旧的截图，只保留最近的 N 张
        /// </summary>
        private static void CleanupOldScreenshots()
        {
            if (!_saveDebugScreenshots) return;

            try
            {
                if (string.IsNullOrEmpty(_screenshotFolder))
                {
                    _screenshotFolder = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Screenshots"
                    );
                }

                if (!Directory.Exists(_screenshotFolder)) return;

                // 获取所有截图文件（按创建时间排序）
                var screenshotFiles = Directory.GetFiles(_screenshotFolder, "*.png")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                // 删除多余的文件
                for (int i = _maxScreenshotsToKeep; i < screenshotFiles.Count; i++)
                {
                    try
                    {
                        File.Delete(screenshotFiles[i].FullName);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"删除旧截图失败: {screenshotFiles[i].Name}, 错误: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清理截图失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存调试截图（根据配置决定是否保存）
        /// </summary>
        private static void SaveDebugScreenshot(Bitmap screenshot, string imageName)
        {
            // 加载配置
            LoadConfig();

            // 如果配置了不保存，直接返回
            if (!_saveDebugScreenshots) return;

            try
            {
                if (_screenshotFolder == null)
                {
                    _screenshotFolder = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Images",
                        "Screenshots"
                    );
                }

                if (!Directory.Exists(_screenshotFolder))
                {
                    Directory.CreateDirectory(_screenshotFolder);
                }

                string fileName = $"{imageName}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
                string filePath = Path.Combine(_screenshotFolder, fileName);
                screenshot.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                // 每次保存后清理旧文件
                CleanupOldScreenshots();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存调试截图失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 截取全屏
        /// </summary>
        public Bitmap CaptureFullScreen()
        {
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size);
            }
            return bmp;
        }

        /// <summary>
        /// 截取指定区域
        /// </summary>
        public Bitmap CaptureRegion(Rectangle rect)
        {
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size);
            }
            return bmp;
        }

        /// <summary>
        /// 在屏幕中查找图片（支持缩放）
        /// </summary>
        public Point? FindImage(Bitmap templateBitmap, Bitmap sourceBitmap = null, double threshold = 0.8, double scaleRange = 0.2)
        {
            var results = FindAllImages(templateBitmap, sourceBitmap, threshold, scaleRange, 1);
            return results.Count > 0 ? results[0] : (Point?)null;
        }

        /// <summary>
        /// 查找所有匹配的图片
        /// </summary>
        public List<Point> FindAllImages(Bitmap templateBitmap, Bitmap sourceBitmap = null, double threshold = 0.8, double scaleRange = 0.2, int maxResults = 10)
        {
            List<Point> results = new List<Point>();

            if (sourceBitmap == null)
            {
                sourceBitmap = CaptureFullScreen();
            }

            using (Mat sourceMat = BitmapConverter.ToMat(sourceBitmap))
            using (Mat templateMat = BitmapConverter.ToMat(templateBitmap))
            using (Mat sourceGray = new Mat())
            using (Mat templateGray = new Mat())
            {
                Cv2.CvtColor(sourceMat, sourceGray, ColorConversionCodes.BGR2GRAY);
                Cv2.CvtColor(templateMat, templateGray, ColorConversionCodes.BGR2GRAY);

                double minScale = 1.0 - scaleRange;
                double maxScale = 1.0 + scaleRange;
                double step = 0.05;

                List<MatchResult> allMatches = new List<MatchResult>();

                for (double scale = minScale; scale <= maxScale; scale += step)
                {
                    int newWidth = (int)(templateGray.Width * scale);
                    int newHeight = (int)(templateGray.Height * scale);

                    if (newWidth > sourceGray.Width || newHeight > sourceGray.Height)
                        continue;

                    using (Mat scaledTemplate = new Mat())
                    {
                        Cv2.Resize(templateGray, scaledTemplate, new OpenCvSharp.Size(newWidth, newHeight));

                        using (Mat result = new Mat())
                        {
                            Cv2.MatchTemplate(sourceGray, scaledTemplate, result, TemplateMatchModes.CCoeffNormed);

                            while (true)
                            {
                                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

                                if (maxVal < threshold)
                                    break;

                                int centerX = maxLoc.X + newWidth / 2;
                                int centerY = maxLoc.Y + newHeight / 2;

                                allMatches.Add(new MatchResult
                                {
                                    X = centerX,
                                    Y = centerY,
                                    Scale = scale,
                                    Similarity = maxVal
                                });

                                int coverSize = 20;
                                OpenCvSharp.Rect coverRect = new OpenCvSharp.Rect(
                                    Math.Max(0, maxLoc.X - coverSize),
                                    Math.Max(0, maxLoc.Y - coverSize),
                                    Math.Min(result.Width - maxLoc.X + coverSize, newWidth + coverSize * 2),
                                    Math.Min(result.Height - maxLoc.Y + coverSize, newHeight + coverSize * 2)
                                );
                                Cv2.Rectangle(result, coverRect, Scalar.Black, -1);
                            }
                        }
                    }
                }

                var sorted = allMatches.OrderByDescending(m => m.Similarity).ToList();
                foreach (var match in sorted)
                {
                    bool tooClose = false;
                    foreach (var existing in results)
                    {
                        int distance = (int)Math.Sqrt(Math.Pow(match.X - existing.X, 2) + Math.Pow(match.Y - existing.Y, 2));
                        if (distance < 30)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        results.Add(new Point(match.X, match.Y));
                        if (results.Count >= maxResults)
                            break;
                    }
                }
            }

            if (sourceBitmap != null && sourceBitmap != CaptureFullScreen())
            {
                sourceBitmap.Dispose();
            }

            return results;
        }

        /// <summary>
        /// 保存全屏截图（调试用，根据配置决定是否保存）
        /// </summary>
        public void SaveScreenshot(string filePath)
        {
            using (Bitmap bmp = CaptureFullScreen())
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                SaveDebugScreenshot(bmp, fileName);
            }
        }

        /// <summary>
        /// 保存区域截图（调试用，根据配置决定是否保存）
        /// </summary>
        public void SaveRegionScreenshot(Rectangle rect, string filePath)
        {
            using (Bitmap bmp = CaptureRegion(rect))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                SaveDebugScreenshot(bmp, fileName);
            }
        }

        private class MatchResult
        {
            public int X { get; set; }
            public int Y { get; set; }
            public double Scale { get; set; }
            public double Similarity { get; set; }
        }
    }
}