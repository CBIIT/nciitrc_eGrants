namespace OutlookAccessADO
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            button1 = new Button();
            outputTextBox = new TextBox();
            interOpButton = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(64, 50);
            label1.Name = "label1";
            label1.Size = new Size(73, 15);
            label1.TabIndex = 0;
            label1.Text = "Outlook Test";
            label1.Click += label1_Click;
            // 
            // button1
            // 
            button1.Location = new Point(216, 46);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 1;
            button1.Text = "Use OLE";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // outputTextBox
            // 
            outputTextBox.Location = new Point(64, 98);
            outputTextBox.Multiline = true;
            outputTextBox.Name = "outputTextBox";
            outputTextBox.Size = new Size(527, 166);
            outputTextBox.TabIndex = 2;
            // 
            // interOpButton
            // 
            interOpButton.Location = new Point(327, 50);
            interOpButton.Name = "interOpButton";
            interOpButton.Size = new Size(75, 23);
            interOpButton.TabIndex = 3;
            interOpButton.Text = "InterOp";
            interOpButton.UseVisualStyleBackColor = true;
            interOpButton.Click += interOpButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(643, 300);
            Controls.Add(interOpButton);
            Controls.Add(outputTextBox);
            Controls.Add(button1);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button button1;
        private TextBox outputTextBox;
        private Button interOpButton;
    }
}
