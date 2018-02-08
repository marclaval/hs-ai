using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.API;

namespace HS_AI_PDT_Plugin
{
    /// <summary>
    /// Interaction logic for Messenger.xaml
    /// </summary>
    public partial class Messenger : Window
    {
        private string _title = "";
        private List<string> _messages;
        private GameEventsHandler _gameEventsHandler;

        public Messenger()
        {
            _messages = new List<string>();
            InitializeComponent();
            Refresh();
        }

        public void SetTitle(string title)
        {
            _title = title;
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

        internal void SetGameEventsHandler(GameEventsHandler gameEventsHandler)
        {
            _gameEventsHandler = gameEventsHandler;
        }

        private void Refresh()
        {
            var tmp = new List<string>(_messages);
            tmp.Insert(0, _title);
            this.msgBinding.ItemsSource = tmp;
        }

        private void ButtonProcess_Click(object sender, RoutedEventArgs e)
        {
            _gameEventsHandler.ProcessPlayerAction();
        }
    }
}
