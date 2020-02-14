namespace PylonLiveViewControl
{
    partial class IntSliderUserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelName = new System.Windows.Forms.Label();
            this.labelCurrentValue = new System.Windows.Forms.Label();
            this.labelMin = new System.Windows.Forms.Label();
            this.labelMax = new System.Windows.Forms.Label();
            this.slider = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.slider)).BeginInit();
            this.SuspendLayout();
            //
            // labelName
            //
            this.labelName.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.labelName.Location = new System.Drawing.Point(0, 30);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(128, 13);
            this.labelName.TabIndex = 1;
            this.labelName.Text = "ValueName:";
            this.labelName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //
            // labelCurrentValue
            //
            this.labelCurrentValue.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.labelCurrentValue.AutoSize = true;
            this.labelCurrentValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelCurrentValue.Location = new System.Drawing.Point(134, 30);
            this.labelCurrentValue.Name = "labelCurrentValue";
            this.labelCurrentValue.Size = new System.Drawing.Size(15, 15);
            this.labelCurrentValue.TabIndex = 1;
            this.labelCurrentValue.Text = "0";
            //
            // labelMin
            //
            this.labelMin.AutoSize = true;
            this.labelMin.Location = new System.Drawing.Point(10, 14);
            this.labelMin.Name = "labelMin";
            this.labelMin.Size = new System.Drawing.Size(24, 13);
            this.labelMin.TabIndex = 1;
            this.labelMin.Text = "Min";
            this.labelMin.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //
            // labelMax
            //
            this.labelMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMax.AutoSize = true;
            this.labelMax.Location = new System.Drawing.Point(185, 14);
            this.labelMax.Name = "labelMax";
            this.labelMax.Size = new System.Drawing.Size(27, 13);
            this.labelMax.TabIndex = 1;
            this.labelMax.Text = "Max";
            //
            // slider
            //
            this.slider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.slider.Location = new System.Drawing.Point(30, 0);
            this.slider.Name = "slider";
            this.slider.Size = new System.Drawing.Size(155, 45);
            this.slider.TabIndex = 0;
            this.slider.Scroll += new System.EventHandler(this.slider_Scroll);
            //
            // IntSliderUserControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.labelCurrentValue);
            this.Controls.Add(this.labelMax);
            this.Controls.Add(this.labelMin);
            this.Controls.Add(this.slider);
            this.MinimumSize = new System.Drawing.Size(245, 50);
            this.Name = "IntSliderUserControl";
            this.Size = new System.Drawing.Size(245, 50);
            ((System.ComponentModel.ISupportInitialize)(this.slider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelCurrentValue;
        private System.Windows.Forms.Label labelMin;
        private System.Windows.Forms.Label labelMax;
        private System.Windows.Forms.TrackBar slider;
    }
}
