﻿namespace plot
{
    partial class GraphiqueUtilisateur
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
            this.SuspendLayout();
            // 
            // ScatterPlot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ScatterPlot";
            this.SizeChanged += new System.EventHandler(this.GraphiqueUtilisateur_SizeChanged);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GraphiqueUtilisateur_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GraphiqueUtilisateur_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GraphiqueUtilisateur_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
