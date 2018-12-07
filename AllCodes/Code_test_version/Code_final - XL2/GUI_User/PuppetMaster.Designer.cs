namespace Projeto_DAD
{
    partial class PuppetMaster
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox_Browse = new System.Windows.Forms.TextBox();
            this.button_Browse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_Run = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button_NextStep = new System.Windows.Forms.Button();
            this.button_Sequence = new System.Windows.Forms.Button();
            this.button_Step = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_Browse
            // 
            this.textBox_Browse.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Browse.Location = new System.Drawing.Point(345, 25);
            this.textBox_Browse.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox_Browse.Name = "textBox_Browse";
            this.textBox_Browse.Size = new System.Drawing.Size(536, 24);
            this.textBox_Browse.TabIndex = 0;
            // 
            // button_Browse
            // 
            this.button_Browse.BackColor = System.Drawing.SystemColors.HighlightText;
            this.button_Browse.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Browse.Location = new System.Drawing.Point(211, 18);
            this.button_Browse.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Browse.Name = "button_Browse";
            this.button_Browse.Size = new System.Drawing.Size(128, 34);
            this.button_Browse.TabIndex = 1;
            this.button_Browse.Text = "Browse";
            this.button_Browse.UseVisualStyleBackColor = false;
            this.button_Browse.Click += new System.EventHandler(this.button_Browse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(167, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select the script";
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(12, 146);
            this.textBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(1137, 159);
            this.textBox2.TabIndex = 3;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 25);
            this.label2.TabIndex = 4;
            this.label2.Text = "Write a command";
            // 
            // button_Run
            // 
            this.button_Run.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.button_Run.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Run.Location = new System.Drawing.Point(211, 95);
            this.button_Run.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Run.Name = "button_Run";
            this.button_Run.Size = new System.Drawing.Size(128, 34);
            this.button_Run.TabIndex = 5;
            this.button_Run.Text = "Run";
            this.button_Run.UseVisualStyleBackColor = false;
            this.button_Run.Click += new System.EventHandler(this.button_Run_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // button_NextStep
            // 
            this.button_NextStep.BackColor = System.Drawing.Color.LightSalmon;
            this.button_NextStep.Enabled = false;
            this.button_NextStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_NextStep.Location = new System.Drawing.Point(887, 56);
            this.button_NextStep.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_NextStep.Name = "button_NextStep";
            this.button_NextStep.Size = new System.Drawing.Size(262, 34);
            this.button_NextStep.TabIndex = 8;
            this.button_NextStep.Text = "Next Step";
            this.button_NextStep.UseVisualStyleBackColor = false;
            this.button_NextStep.Click += new System.EventHandler(this.button_NextStep_Click);
            // 
            // button_Sequence
            // 
            this.button_Sequence.BackColor = System.Drawing.SystemColors.HighlightText;
            this.button_Sequence.Enabled = false;
            this.button_Sequence.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Sequence.Location = new System.Drawing.Point(887, 18);
            this.button_Sequence.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Sequence.Name = "button_Sequence";
            this.button_Sequence.Size = new System.Drawing.Size(146, 34);
            this.button_Sequence.TabIndex = 9;
            this.button_Sequence.Text = "Sequence";
            this.button_Sequence.UseVisualStyleBackColor = false;
            this.button_Sequence.Click += new System.EventHandler(this.button_Sequence_Click);
            // 
            // button_Step
            // 
            this.button_Step.BackColor = System.Drawing.SystemColors.HighlightText;
            this.button_Step.Enabled = false;
            this.button_Step.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Step.Location = new System.Drawing.Point(1039, 18);
            this.button_Step.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Step.Name = "button_Step";
            this.button_Step.Size = new System.Drawing.Size(110, 34);
            this.button_Step.TabIndex = 10;
            this.button_Step.Text = "Step";
            this.button_Step.UseVisualStyleBackColor = false;
            this.button_Step.Click += new System.EventHandler(this.button_Step_Click);
            // 
            // PuppetMaster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1177, 318);
            this.Controls.Add(this.button_Step);
            this.Controls.Add(this.button_Sequence);
            this.Controls.Add(this.button_NextStep);
            this.Controls.Add(this.button_Run);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Browse);
            this.Controls.Add(this.textBox_Browse);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "PuppetMaster";
            this.Text = "PuppetMaster";
            this.Load += new System.EventHandler(this.GUI_Client_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_Browse;
        private System.Windows.Forms.Button button_Browse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_Run;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button_NextStep;
        private System.Windows.Forms.Button button_Sequence;
        private System.Windows.Forms.Button button_Step;
    }
}

