using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TianXiaMiner.Utils
{
    /// <summary>
    /// 日志帮助类 - 统一管理所有日志输出
    /// 支持 ListView 控件显示 + 文件保存
    /// </summary>
    public static class LogHelper
    {
        private static ListView _listLog;
        private static string _logFilePath;
        private static int _maxLogFiles = 3; // 只保留最新的3个日志文件
        private static object _lockObj = new object();

        /// <summary>
        /// 初始化日志控件（在 MainForm_Load 中调用）
        /// </summary>
        public static void Initialize(ListView listLog)
        {
            _listLog = listLog;
            _listLog.View = View.Details;
            _listLog.Columns.Clear();
            _listLog.Columns.Add("时间", 80, HorizontalAlignment.Left);
            _listLog.Columns.Add("消息", 500, HorizontalAlignment.Left);
            _listLog.LabelWrap = false;
            _listLog.Scrollable = true;
            _listLog.FullRowSelect = true;
            _listLog.GridLines = true;

            // 初始化日志文件
            InitializeLogFile();

            // 清理旧日志文件
            CleanupOldLogFiles();
        }

        /// <summary>
        /// 初始化日志文件
        /// </summary>
        private static void InitializeLogFile()
        {
            try
            {
                string logDir = Path.Combine(Application.StartupPath, "Logs");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                string fileName = $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                _logFilePath = Path.Combine(logDir, fileName);

                // 写入文件头
                File.AppendAllText(_logFilePath, $"=== 日志开始 {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\r\n");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化日志文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理旧日志文件，只保留最新的3个
        /// </summary>
        private static void CleanupOldLogFiles()
        {
            try
            {
                string logDir = Path.Combine(Application.StartupPath, "Logs");
                if (!Directory.Exists(logDir)) return;

                // 获取所有日志文件并按创建时间排序
                var logFiles = Directory.GetFiles(logDir, "log_*.txt")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                // 删除多余的文件（保留最新的_maxLogFiles个）
                for (int i = _maxLogFiles; i < logFiles.Count; i++)
                {
                    File.Delete(logFiles[i].FullName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清理日志文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 写入日志（自动换行，带时间戳）
        /// </summary>
        public static void Log(string message)
        {
            Log(message, Color.Black);
        }

        /// <summary>
        /// 写入日志（指定颜色）
        /// </summary>
        public static void Log(string message, Color color)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logMessage = $"[{timestamp}] {message}";

            // 显示到 ListView
            if (_listLog != null)
            {
                if (_listLog.InvokeRequired)
                {
                    _listLog.Invoke(new Action(() => AddToListView(timestamp, message, color)));
                }
                else
                {
                    AddToListView(timestamp, message, color);
                }
            }

            // 写入日志文件
            WriteToFile(logMessage);
        }

        /// <summary>
        /// 添加日志到 ListView（自动滚动到底部）
        /// </summary>
        private static void AddToListView(string timestamp, string message, Color color)
        {
            if (_listLog == null) return;

            // 在顶部插入最新日志（和原来一样）
            ListViewItem item = new ListViewItem(timestamp);
            item.SubItems.Add(message);
            item.ForeColor = color;
            _listLog.Items.Insert(0, item);

            // 可选：限制日志数量，防止内存溢出（保留最近1000条）
            while (_listLog.Items.Count > 1000)
            {
                _listLog.Items.RemoveAt(_listLog.Items.Count - 1);
            }
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        private static void WriteToFile(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(_logFilePath))
                {
                    lock (_lockObj)
                    {
                        File.AppendAllText(_logFilePath, message + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"写入日志文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 写入轮次分隔线
        /// </summary>
        public static void LogRoundComplete(int roundNumber)
        {
            string message = $"\r\n=== 第 {roundNumber} 轮完成 {DateTime.Now:HH:mm:ss} ===\r\n";
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string displayMessage = $"=== 第 {roundNumber} 轮完成 ===";

            if (_listLog != null)
            {
                if (_listLog.InvokeRequired)
                {
                    _listLog.Invoke(new Action(() => AddToListView(timestamp, displayMessage, Color.Blue)));
                }
                else
                {
                    AddToListView(timestamp, displayMessage, Color.Blue);
                }
            }

            WriteToFile(message);
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        public static void Clear()
        {
            if (_listLog == null) return;

            if (_listLog.InvokeRequired)
            {
                _listLog.Invoke(new Action(() => _listLog.Items.Clear()));
            }
            else
            {
                _listLog.Items.Clear();
            }

            // 写入文件分隔线
            WriteToFile($"\r\n=== 日志已清空 {DateTime.Now:HH:mm:ss} ===\r\n");
        }

        /// <summary>
        /// 写入错误日志（红色）
        /// </summary>
        public static void LogError(string message)
        {
            Log($"❌ {message}", Color.Red);
        }

        /// <summary>
        /// 写入成功日志（绿色）
        /// </summary>
        public static void LogSuccess(string message)
        {
            Log($"✅ {message}", Color.Green);
        }

        /// <summary>
        /// 写入警告日志（橙色）
        /// </summary>
        public static void LogWarning(string message)
        {
            Log($"⚠️ {message}", Color.Orange);
        }

        /// <summary>
        /// 写入信息日志（蓝色）
        /// </summary>
        public static void LogInfo(string message)
        {
            Log(message, Color.Blue);
        }
    }
}