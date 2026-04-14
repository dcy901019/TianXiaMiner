
namespace TianXiaMiner
{
    partial class Main
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.btnStep1_CheckItem = new System.Windows.Forms.Button();
            this.btnStep2_UseMaterial = new System.Windows.Forms.Button();
            this.btnStep3_FindOnMap = new System.Windows.Forms.Button();
            this.btnStep4_CollectResource = new System.Windows.Forms.Button();
            this.btnStep5_GoHome = new System.Windows.Forms.Button();
            this.btnNextWindow = new System.Windows.Forms.Button();
            this.btnOpenTemplates = new System.Windows.Forms.Button();
            this.btnAutoRun = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStep1_CheckItem
            // 
            this.btnStep1_CheckItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(127)))), ((int)(((byte)(80)))));
            this.btnStep1_CheckItem.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(80)))), ((int)(((byte)(40)))));
            this.btnStep1_CheckItem.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(90)))), ((int)(((byte)(50)))));
            this.btnStep1_CheckItem.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(160)))), ((int)(((byte)(120)))));
            this.btnStep1_CheckItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStep1_CheckItem.Font = new System.Drawing.Font("宋体", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStep1_CheckItem.ForeColor = System.Drawing.Color.White;
            this.btnStep1_CheckItem.Location = new System.Drawing.Point(12, 15);
            this.btnStep1_CheckItem.Name = "btnStep1_CheckItem";
            this.btnStep1_CheckItem.Size = new System.Drawing.Size(40, 30);
            this.btnStep1_CheckItem.TabIndex = 0;
            this.btnStep1_CheckItem.Text = "检查足通";
            this.btnStep1_CheckItem.UseVisualStyleBackColor = false;
            this.btnStep1_CheckItem.Click += new System.EventHandler(this.btnStep1_CheckItem_Click);
            // 
            // btnStep2_UseMaterial
            // 
            this.btnStep2_UseMaterial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(127)))), ((int)(((byte)(80)))));
            this.btnStep2_UseMaterial.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(80)))), ((int)(((byte)(40)))));
            this.btnStep2_UseMaterial.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(90)))), ((int)(((byte)(50)))));
            this.btnStep2_UseMaterial.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(160)))), ((int)(((byte)(120)))));
            this.btnStep2_UseMaterial.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStep2_UseMaterial.Font = new System.Drawing.Font("宋体", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStep2_UseMaterial.ForeColor = System.Drawing.Color.White;
            this.btnStep2_UseMaterial.Location = new System.Drawing.Point(56, 15);
            this.btnStep2_UseMaterial.Name = "btnStep2_UseMaterial";
            this.btnStep2_UseMaterial.Size = new System.Drawing.Size(40, 30);
            this.btnStep2_UseMaterial.TabIndex = 1;
            this.btnStep2_UseMaterial.Text = "放感应材料";
            this.btnStep2_UseMaterial.UseVisualStyleBackColor = false;
            this.btnStep2_UseMaterial.Click += new System.EventHandler(this.btnStep2_UseMaterial_Click);
            // 
            // btnStep3_FindOnMap
            // 
            this.btnStep3_FindOnMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(127)))), ((int)(((byte)(80)))));
            this.btnStep3_FindOnMap.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(80)))), ((int)(((byte)(40)))));
            this.btnStep3_FindOnMap.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(90)))), ((int)(((byte)(50)))));
            this.btnStep3_FindOnMap.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(160)))), ((int)(((byte)(120)))));
            this.btnStep3_FindOnMap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStep3_FindOnMap.Font = new System.Drawing.Font("宋体", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStep3_FindOnMap.ForeColor = System.Drawing.Color.White;
            this.btnStep3_FindOnMap.Location = new System.Drawing.Point(100, 15);
            this.btnStep3_FindOnMap.Name = "btnStep3_FindOnMap";
            this.btnStep3_FindOnMap.Size = new System.Drawing.Size(40, 30);
            this.btnStep3_FindOnMap.TabIndex = 2;
            this.btnStep3_FindOnMap.Text = "地图找资源";
            this.btnStep3_FindOnMap.UseVisualStyleBackColor = false;
            this.btnStep3_FindOnMap.Click += new System.EventHandler(this.btnStep3_FindOnMap_Click);
            // 
            // btnStep4_CollectResource
            // 
            this.btnStep4_CollectResource.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(127)))), ((int)(((byte)(80)))));
            this.btnStep4_CollectResource.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(80)))), ((int)(((byte)(40)))));
            this.btnStep4_CollectResource.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(90)))), ((int)(((byte)(50)))));
            this.btnStep4_CollectResource.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(160)))), ((int)(((byte)(120)))));
            this.btnStep4_CollectResource.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStep4_CollectResource.Font = new System.Drawing.Font("宋体", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStep4_CollectResource.ForeColor = System.Drawing.Color.White;
            this.btnStep4_CollectResource.Location = new System.Drawing.Point(144, 15);
            this.btnStep4_CollectResource.Name = "btnStep4_CollectResource";
            this.btnStep4_CollectResource.Size = new System.Drawing.Size(40, 30);
            this.btnStep4_CollectResource.TabIndex = 3;
            this.btnStep4_CollectResource.Text = "采集资源";
            this.btnStep4_CollectResource.UseVisualStyleBackColor = false;
            this.btnStep4_CollectResource.Click += new System.EventHandler(this.btnStep4_CollectResource_Click);
            // 
            // btnStep5_GoHome
            // 
            this.btnStep5_GoHome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(127)))), ((int)(((byte)(80)))));
            this.btnStep5_GoHome.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(80)))), ((int)(((byte)(40)))));
            this.btnStep5_GoHome.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(90)))), ((int)(((byte)(50)))));
            this.btnStep5_GoHome.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(160)))), ((int)(((byte)(120)))));
            this.btnStep5_GoHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStep5_GoHome.Font = new System.Drawing.Font("宋体", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStep5_GoHome.ForeColor = System.Drawing.Color.White;
            this.btnStep5_GoHome.Location = new System.Drawing.Point(188, 15);
            this.btnStep5_GoHome.Name = "btnStep5_GoHome";
            this.btnStep5_GoHome.Size = new System.Drawing.Size(40, 30);
            this.btnStep5_GoHome.TabIndex = 4;
            this.btnStep5_GoHome.Text = "回城";
            this.btnStep5_GoHome.UseVisualStyleBackColor = false;
            this.btnStep5_GoHome.Click += new System.EventHandler(this.btnStep5_GoHome_Click);
            // 
            // btnNextWindow
            // 
            this.btnNextWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(127)))), ((int)(((byte)(80)))));
            this.btnNextWindow.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(80)))), ((int)(((byte)(40)))));
            this.btnNextWindow.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(90)))), ((int)(((byte)(50)))));
            this.btnNextWindow.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(160)))), ((int)(((byte)(120)))));
            this.btnNextWindow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextWindow.Font = new System.Drawing.Font("宋体", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnNextWindow.ForeColor = System.Drawing.Color.White;
            this.btnNextWindow.Location = new System.Drawing.Point(232, 15);
            this.btnNextWindow.Name = "btnNextWindow";
            this.btnNextWindow.Size = new System.Drawing.Size(40, 30);
            this.btnNextWindow.TabIndex = 6;
            this.btnNextWindow.Text = "切换窗口";
            this.btnNextWindow.UseVisualStyleBackColor = false;
            this.btnNextWindow.Click += new System.EventHandler(this.btnNextWindow_Click);
            // 
            // btnOpenTemplates
            // 
            this.btnOpenTemplates.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(127)))), ((int)(((byte)(80)))));
            this.btnOpenTemplates.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(80)))), ((int)(((byte)(40)))));
            this.btnOpenTemplates.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(90)))), ((int)(((byte)(50)))));
            this.btnOpenTemplates.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(160)))), ((int)(((byte)(120)))));
            this.btnOpenTemplates.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenTemplates.Font = new System.Drawing.Font("宋体", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOpenTemplates.ForeColor = System.Drawing.Color.White;
            this.btnOpenTemplates.Location = new System.Drawing.Point(276, 15);
            this.btnOpenTemplates.Name = "btnOpenTemplates";
            this.btnOpenTemplates.Size = new System.Drawing.Size(40, 30);
            this.btnOpenTemplates.TabIndex = 7;
            this.btnOpenTemplates.Text = "图片文件夹";
            this.btnOpenTemplates.UseVisualStyleBackColor = false;
            this.btnOpenTemplates.Click += new System.EventHandler(this.btnOpenTemplates_Click);
            // 
            // btnAutoRun
            // 
            this.btnAutoRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(139)))), ((int)(((byte)(87)))));
            this.btnAutoRun.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(100)))), ((int)(((byte)(60)))));
            this.btnAutoRun.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(110)))), ((int)(((byte)(70)))));
            this.btnAutoRun.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(170)))), ((int)(((byte)(100)))));
            this.btnAutoRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAutoRun.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAutoRun.ForeColor = System.Drawing.Color.White;
            this.btnAutoRun.Location = new System.Drawing.Point(149, 51);
            this.btnAutoRun.Name = "btnAutoRun";
            this.btnAutoRun.Size = new System.Drawing.Size(52, 32);
            this.btnAutoRun.TabIndex = 8;
            this.btnAutoRun.Text = "▶ 运行";
            this.btnAutoRun.UseVisualStyleBackColor = false;
            this.btnAutoRun.Click += new System.EventHandler(this.btnAutoRun_Click);
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.White;
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLog.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.txtLog.Location = new System.Drawing.Point(12, 112);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(313, 184);
            this.txtLog.TabIndex = 10;
            // 
            // btnPause
            // 
            this.btnPause.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.btnPause.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(90)))), ((int)(((byte)(140)))));
            this.btnPause.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(100)))), ((int)(((byte)(150)))));
            this.btnPause.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(160)))), ((int)(((byte)(210)))));
            this.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPause.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnPause.ForeColor = System.Drawing.Color.White;
            this.btnPause.Location = new System.Drawing.Point(207, 51);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(52, 32);
            this.btnPause.TabIndex = 12;
            this.btnPause.Text = "⏸️暂停";
            this.btnPause.UseVisualStyleBackColor = false;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
            this.btnStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.btnStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.btnStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Location = new System.Drawing.Point(265, 51);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(52, 32);
            this.btnStop.TabIndex = 13;
            this.btnStop.Text = "⏹️停止";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(332, 308);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnAutoRun);
            this.Controls.Add(this.btnOpenTemplates);
            this.Controls.Add(this.btnNextWindow);
            this.Controls.Add(this.btnStep5_GoHome);
            this.Controls.Add(this.btnStep4_CollectResource);
            this.Controls.Add(this.btnStep3_FindOnMap);
            this.Controls.Add(this.btnStep2_UseMaterial);
            this.Controls.Add(this.btnStep1_CheckItem);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TianXiaMiner";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnAutoRun;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStep1_CheckItem;
        private System.Windows.Forms.Button btnStep2_UseMaterial;
        private System.Windows.Forms.Button btnStep3_FindOnMap;
        private System.Windows.Forms.Button btnStep4_CollectResource;
        private System.Windows.Forms.Button btnStep5_GoHome;
        private System.Windows.Forms.Button btnNextWindow;
        private System.Windows.Forms.Button btnOpenTemplates;
    }
}

