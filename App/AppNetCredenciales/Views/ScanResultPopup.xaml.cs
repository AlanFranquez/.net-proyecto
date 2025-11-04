using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System;

namespace AppNetCredenciales.Views
{
    public partial class ScanResultPopup : Popup
    {
        public string IconSymbol { get; set; }
        public Color IconBackgroundColor { get; set; }
        public Color ButtonColor { get; set; }

        public ScanResultPopup(string title, string message, bool success)
        {
            // Ensure this method exists in the generated partial class
            this.InitializeComponent();

            TitleLabel.Text = title;
            MessageLabel.Text = message;

            if (success)
            {
                IconSymbol = "✔️";
                IconBackgroundColor = Colors.Green;
                ButtonColor = Color.FromArgb("#8E6FF7");
            }
            else
            {
                IconSymbol = "❌";
                IconBackgroundColor = Colors.Red;
                ButtonColor = Color.FromArgb("#D66E6E");
            }

            BindingContext = this;
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}