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
        public Messenger()
        {
            InitializeComponent();
        }

        public void Update(List<string> messages)
        {
            this.Visibility = messages.Count <= 0 ? Visibility.Hidden : Visibility.Visible;
            this.msgBinding.ItemsSource = messages;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
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
