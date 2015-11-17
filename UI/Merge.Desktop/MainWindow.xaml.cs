using System;
using Merge.Desktop.Extentions;

namespace Merge.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            SourceInitialized += OnSourceInitialized;

            InitializeComponent();
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            this.InitializeButtons();
        }
    }
}
