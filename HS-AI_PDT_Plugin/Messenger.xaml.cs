using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.API;

namespace HS_AI_PDT_Plugin
{
    /// <summary>
    /// Interaction logic for Messenger.xaml
    /// </summary>
    public partial class Messenger : UserControl
    {
        private List<string> _messages;
        public Messenger()
        {
            _messages = new List<string>();
            InitializeComponent();
            Refresh();
        }

        public void Set(List<string> messages)
        {
            _messages = messages;
            Refresh();
        }

        public void Add(string message)
        {
            _messages.Add(message);
            Refresh();
        }

        public void Reset()
        {
            _messages = new List<string>();
            Refresh();
        }

        private void Refresh()
        {
            this.msgBinding.ItemsSource = new List<string>(_messages);
            Canvas.SetBottom(this, 0);
            Canvas.SetLeft(this, 0);
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
