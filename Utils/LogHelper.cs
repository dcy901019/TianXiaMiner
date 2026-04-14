using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TianXiaMiner.Utils
{
    /// <summary>
    /// 日志帮助类 - 统一管理所有日志输出
    /// </summary>
    public static class LogHelper
    {
        private static TextBox _txtLog;
        private static string _logFilePath;
        private static int _maxLogFiles = 3; // 只保留最新的3个日志文件

        /// <summary>
        /// 初始化日志控件（在Form1_Load中调用）
        /// </summary>
        public static void Initialize(TextBox txtLog)
        {
            _txtLog = txtLog;
            _txtLog.WordWrap = true;
            _txtLog.ReadOnly = true;
            _txtLog.BackColor = System.Drawing.Color.White;

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
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logMessage = $"[{timestamp}] {message}";

            // 显示到文本框
            if (_txtLog == null)
            {
                System.Diagnostics.Debug.WriteLine($"日志控件未初始化: {message}");
                return;
            }

            if (_txtLog.InvokeRequired)
            {
                _txtLog.Invoke(new Action(() => {
                    _txtLog.AppendText(logMessage + Environment.NewLine);
                }));
            }
            else
            {
                _txtLog.AppendText(logMessage + Environment.NewLine);
            }

            // 写入日志文件
            WriteToFile(logMessage);
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
                    File.AppendAllText(_logFilePath, message + Environment.NewLine);
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

            if (_txtLog != null)
            {
                if (_txtLog.InvokeRequired)
                {
                    _txtLog.Invoke(new Action(() => {
                        _txtLog.AppendText(message);
                    }));
                }
                else
                {
                    _txtLog.AppendText(message);
                }
            }

            WriteToFile(message);
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        public static void Clear()
        {
            if (_txtLog == null) return;

            if (_txtLog.InvokeRequired)
            {
                _txtLog.Invoke(new Action(() => _txtLog.Clear()));
            }
            else
            {
                _txtLog.Clear();
            }

            // 写入文件分隔线
            WriteToFile($"\r\n=== 日志已清空 {DateTime.Now:HH:mm:ss} ===\r\n");
        }

        /// <summary>
        /// 写入错误日志
        /// </summary>
        public static void LogError(string message)
        {
            Log($"❌ {message}");
        }

        /// <summary>
        /// 写入成功日志
        /// </summary>
        public static void LogSuccess(string message)
        {
            Log($"✅ {message}");
        }

        /// <summary>
        /// 写入警告日志
        /// </summary>
        public static void LogWarning(string message)
        {
            Log($"⚠️ {message}");
        }
    }
}