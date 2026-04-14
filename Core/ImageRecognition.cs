using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = System.Drawing.Point;

namespace TianXiaMiner.Core
{
    /// <summary>
    /// 图片识别�?- 使用OpenCvSharp4
    /// 支持缩放识别，多目标识别，相似度匹配
    /// </summary>
    public class ImageRecognition
    {
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
        /// <param name="templateBitmap">要查找的模板图片</param>
        /// <param name="sourceBitmap">源图片（null则截全屏�?/param>
        /// <param name="threshold">匹配阈�?0-1，推�?.8</param>
        /// <param name="scaleRange">缩放范围�?.2表示0.8倍到1.2�?/param>
        /// <returns>找到的中心点坐标，没找到返回null</returns>
        public Point? FindImage(Bitmap templateBitmap, Bitmap sourceBitmap = null, double threshold = 0.8, double scaleRange = 0.2)
        {
            var results = FindAllImages(templateBitmap, sourceBitmap, threshold, scaleRange, 1);
            return results.Count > 0 ? results[0] : (Point?)null;
        }

        /// <summary>
        /// 查找所有匹配的图片（多个相同矿石）
        /// </summary>
        /// <param name="templateBitmap">模板图片</param>
        /// <param name="sourceBitmap">源图�?/param>
        /// <param name="threshold">匹配阈�?/param>
        /// <param name="scaleRange">缩放范围</param>
        /// <param name="maxResults">最多返回几个结�?/param>
        /// <returns>中心点坐标列�?/returns>
        public List<Point> FindAllImages(Bitmap templateBitmap, Bitmap sourceBitmap = null, double threshold = 0.8, double scaleRange = 0.2, int maxResults = 10)
        {
            List<Point> results = new List<Point>();

            // 如果没有传入源图片，截取全屏
            if (sourceBitmap == null)
            {
                sourceBitmap = CaptureFullScreen();
            }

            // 将Bitmap转换为OpenCV的Mat
            using (Mat sourceMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(sourceBitmap))
            using (Mat templateMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(templateBitmap))
            using (Mat sourceGray = new Mat())
            using (Mat templateGray = new Mat())
            {
                // 转为灰度�?
                Cv2.CvtColor(sourceMat, sourceGray, ColorConversionCodes.BGR2GRAY);
                Cv2.CvtColor(templateMat, templateGray, ColorConversionCodes.BGR2GRAY);

                // 尝试不同缩放比例
                double minScale = 1.0 - scaleRange;
                double maxScale = 1.0 + scaleRange;
                double step = 0.05; // 5%步进

                // 存储所有匹配结�?
                List<MatchResult> allMatches = new List<MatchResult>();

                for (double scale = minScale; scale <= maxScale; scale += step)
                {
                    // 缩放模板
                    int newWidth = (int)(templateGray.Width * scale);
                    int newHeight = (int)(templateGray.Height * scale);

                    if (newWidth > sourceGray.Width || newHeight > sourceGray.Height)
                        continue;

                    using (Mat scaledTemplate = new Mat())
                    {
                        Cv2.Resize(templateGray, scaledTemplate, new OpenCvSharp.Size(newWidth, newHeight));

                        // 模板匹配
                        using (Mat result = new Mat())
                        {
                            Cv2.MatchTemplate(sourceGray, scaledTemplate, result, TemplateMatchModes.CCoeffNormed);

                            // 获取匹配结果
                            while (true)
                            {
                                // 找到最大值位�?
                                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

                                if (maxVal < threshold)
                                    break;

                                // 计算中心�?
                                int centerX = maxLoc.X + newWidth / 2;
                                int centerY = maxLoc.Y + newHeight / 2;

                                allMatches.Add(new MatchResult
                                {
                                    X = centerX,
                                    Y = centerY,
                                    Scale = scale,
                                    Similarity = maxVal
                                });

                                // 覆盖这个区域，避免重复找到同一�?
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

                // 按相似度排序，去重（距离太近的只保留一个）
                var sorted = allMatches.OrderByDescending(m => m.Similarity).ToList();
                foreach (var match in sorted)
                {
                    bool tooClose = false;
                    foreach (var existing in results)
                    {
                        int distance = (int)Math.Sqrt(Math.Pow(match.X - existing.X, 2) + Math.Pow(match.Y - existing.Y, 2));
                        if (distance < 30) // 30像素内算同一�?
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

            // 如果不是自己创建的sourceBitmap，需要释�?
            if (sourceBitmap != null && sourceBitmap != CaptureFullScreen())
            {
                sourceBitmap.Dispose();
            }

            return results;
        }

        /// <summary>
        /// 根据颜色查找区域（比如找小地图上的绿点）
        /// </summary>
        public List<Point> FindColor(Bitmap sourceBitmap, Color targetColor, int tolerance = 20)
        {
            List<Point> results = new List<Point>();

            using (Mat sourceMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(sourceBitmap))
            {
                // 转换为HSV色彩空间，更容易处理颜色范围
                using (Mat hsv = new Mat())
                {
                    Cv2.CvtColor(sourceMat, hsv, ColorConversionCodes.BGR2HSV);

                    // 将System.Drawing.Color转换为HSV
                    float hue = targetColor.GetHue();
                    float saturation = targetColor.GetSaturation() * 100; // 转换�?-100范围
                    float value = targetColor.GetBrightness() * 100;

                    // 定义颜色范围
                    Scalar lowerBound = new Scalar(hue - tolerance, 50, 50);
                    Scalar upperBound = new Scalar(hue + tolerance, 255, 255);

                    // 创建掩码
                    using (Mat mask = new Mat())
                    {
                        Cv2.InRange(hsv, lowerBound, upperBound, mask);

                        // 找轮�?
                        Cv2.FindContours(mask, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                        foreach (var contour in contours)
                        {
                            // 计算中心�?
                            Moments m = Cv2.Moments(contour);
                            if (m.M00 > 0)
                            {
                                int cx = (int)(m.M10 / m.M00);
                                int cy = (int)(m.M01 / m.M00);
                                results.Add(new Point(cx, cy));
                            }
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 保存截图（用于调试）
        /// </summary>
        public void SaveScreenshot(string filePath)
        {
            using (Bitmap bmp = CaptureFullScreen())
            {
                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        /// <summary>
        /// 保存区域截图（用于调试）
        /// </summary>
        public void SaveRegionScreenshot(Rectangle rect, string filePath)
        {
            using (Bitmap bmp = CaptureRegion(rect))
            {
                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        /// <summary>
        /// 内部匹配结果�?
        /// </summary>
        private class MatchResult
        {
            public int X { get; set; }
            public int Y { get; set; }
            public double Scale { get; set; }
            public double Similarity { get; set; }
        }
    }
}