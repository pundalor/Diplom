using Avalonia.Controls;
using Avalonia.Interactivity;
using DiplomchikFlowers.Model;
using System.Collections.Generic;
using System.Linq;

namespace DiplomchikFlowers
{
    public partial class StatusSelectionWindow : Window
    {
        private List<OrderStatus> _statuses;
        private int _currentStatusId;

        public StatusSelectionWindow()
        {
            InitializeComponent();
        }

        public StatusSelectionWindow(List<OrderStatus> statuses, int currentStatusId)
        {
            InitializeComponent();
            _statuses = statuses;
            _currentStatusId = currentStatusId;
            LoadStatuses();
        }

        private void LoadStatuses()
        {
            StatusListBox.Items.Clear();
            foreach (var status in _statuses)
            {
                StatusListBox.Items.Add(status);
                if (status.Id == _currentStatusId)
                {
                    StatusListBox.SelectedItem = status;
                }
            }
        }

        private void Ok_Click(object? sender, RoutedEventArgs e)
        {
            if (StatusListBox.SelectedItem is OrderStatus selectedStatus)
            {
                Close(selectedStatus.Id);
            }
            else
            {
                Close(null);
            }
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}