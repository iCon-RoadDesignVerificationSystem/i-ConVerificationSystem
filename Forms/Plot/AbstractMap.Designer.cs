namespace i_ConVerificationSystem.Forms
{
    partial class AbstractMap
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.Chart_Plotarea = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_Plotarea)).BeginInit();
            this.SuspendLayout();
            // 
            // Chart_Plotarea
            // 
            chartArea1.Name = "ChartArea1";
            this.Chart_Plotarea.ChartAreas.Add(chartArea1);
            this.Chart_Plotarea.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.Chart_Plotarea.Legends.Add(legend1);
            this.Chart_Plotarea.Location = new System.Drawing.Point(0, 0);
            this.Chart_Plotarea.Name = "Chart_Plotarea";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.Name = "S1";
            this.Chart_Plotarea.Series.Add(series1);
            this.Chart_Plotarea.Size = new System.Drawing.Size(1140, 567);
            this.Chart_Plotarea.TabIndex = 0;
            this.Chart_Plotarea.Text = "chart1";
            this.Chart_Plotarea.DoubleClick += new System.EventHandler(this.Chart_Plotarea_DoubleClick);
            // 
            // AbstractMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Chart_Plotarea);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "AbstractMap";
            this.Size = new System.Drawing.Size(1140, 567);
            ((System.ComponentModel.ISupportInitialize)(this.Chart_Plotarea)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart Chart_Plotarea;
    }
}
