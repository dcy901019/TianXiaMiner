using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TianXiaMiner.Core
{
    /// <summary>
    /// 延迟管理类 - 从App.config读取配置
    /// </summary>
    public class GameDelay
    {
        private Random _random = new Random();

        // 延迟参数（从配置文件读取）
        private int _minOperationMs;
        private int _maxOperationMs;
        private int _minActionMs;
        private int _maxActionMs;
        private int _minLongMs;
        private int _maxLongMs;

        /// <summary>
        /// 构造函数 - 读取配置文件
        /// </summary>
        public GameDelay()
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
                _minOperationMs = GetConfigInt("MinOperationMs", 50);
                _maxOperationMs = GetConfigInt("MaxOperationMs", 150);
                _minActionMs = GetConfigInt("MinActionMs", 500);
                _maxActionMs = GetConfigInt("MaxActionMs", 700);
                _minLongMs = GetConfigInt("MinLongMs", 1000);
                _maxLongMs = GetConfigInt("MaxLongMs", 2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine("加载配置失败: " + ex.Message);
                // 加载失败时使用默认值
                SetDefaultValues();
            }
        }

        /// <summary>
        /// 从配置文件读取整数
        /// </summary>
        private int GetConfigInt(string key, int defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            if (int.TryParse(value, out int result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        private void SetDefaultValues()
        {
            _minOperationMs = 50;
            _maxOperationMs = 150;
            _minActionMs = 500;
            _maxActionMs = 700;
            _minLongMs = 1000;
            _maxLongMs = 2000;
        }

        /// <summary>
        /// 重新加载配置（修改App.config后调用）
        /// </summary>
        public void ReloadConfig()
        {
            ConfigurationManager.RefreshSection("appSettings");
            LoadConfig();
        }

        /// <summary>
        /// 类型1：操作延迟
        /// </summary>
        public void OperationDelay()
        {
            int ms = _random.Next(_minOperationMs, _maxOperationMs);
            Thread.Sleep(ms);
        }

        /// <summary>
        /// 类型2：动作延迟
        /// </summary>
        public void ActionDelay()
        {
            int ms = _random.Next(_minActionMs, _maxActionMs);
            Thread.Sleep(ms);
        }

        /// <summary>
        /// 类型3：大延迟
        /// </summary>
        public void LongDelay()
        {
            int ms = _random.Next(_minLongMs, _maxLongMs);
            Thread.Sleep(ms);
        }

        /// <summary>
        /// 自定义延迟
        /// </summary>
        public void CustomDelay(int minMs, int maxMs)
        {
            int ms = _random.Next(minMs, maxMs);
            Thread.Sleep(ms);
        }

        /// <summary>
        /// 获取游戏窗口标题
        /// </summary>
        public string GetGameWindowTitle()
        {
            return ConfigurationManager.AppSettings["GameWindowTitle"] ?? "天下3";
        }
    }
}