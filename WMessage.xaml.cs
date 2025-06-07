using System.Windows;

namespace MViewer
{
    /// <summary>
    /// WMessage.xaml 的交互逻辑
    /// </summary>
    public partial class WMessage : Window
    {
        public WMessage()
        {
            InitializeComponent();
        }
        public WMessage(string message)
        {
            InitializeComponent();
            TB_Output.Text = message;
        }
    }
}
