using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ResumeDatabase.lib
{
    class textBoxPlaceholder
    {
        TextBox textBox;
        string placeholder;

        public textBoxPlaceholder(TextBox parentTextBox, string text)
        {
            textBox = parentTextBox;
            placeholder = text;
            textBox.Text = placeholder;
            textBox.MouseEnter += RemoveText;
            textBox.MouseLeave += AddText;
        }

        public void RemoveText(object sender, EventArgs e)
        {
            if (textBox.Text == placeholder) 
            {
                textBox.Text = "";
            }
        }

        public void AddText(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
                textBox.Text = placeholder;
        }
    }
}
