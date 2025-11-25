

using Fakturace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Fakturace
{
    public partial class AddInvoiceWindow : Window
    {

        public Invoice NewInvoice { get; private set; }

        
        private ObservableCollection<InvoiceItem> currentItems = new ObservableCollection<InvoiceItem>();

        
        
        private List<InvoiceItem> availableServices = new List<InvoiceItem>
{
    new InvoiceItem { Description = "Finanční poradenství", HourlyRate = 1200m, HoursWorked = 0 },
    new InvoiceItem { Description = "Osobní coaching", HourlyRate = 1500m, HoursWorked = 0 }
};

        public AddInvoiceWindow()
        {
            InitializeComponent();
            DueDatePicker.SelectedDate = DateTime.Today.AddDays(14);

            
            ServiceComboBox.ItemsSource = availableServices;
            InvoiceItemsListBox.ItemsSource = currentItems;

            
            if (ServiceComboBox.Items.Count > 0)
            {
                ServiceComboBox.SelectedIndex = 0;
            }

            InvoiceNumberTextBox.Focus();
        }

        
        private void ServiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedService = ServiceComboBox.SelectedItem as InvoiceItem;
            if (selectedService != null)
            {
                
                RateTextBox.Text = selectedService.HourlyRate.ToString();
            }
        }

        
        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = ServiceComboBox.SelectedItem as InvoiceItem;

            if (selectedService == null)
            {
                MessageBox.Show("Vyberte službu.", "Chyba");
                return;
            }

            
            if (!int.TryParse(HoursTextBox.Text, out int hours) || hours <= 0)
            {
                MessageBox.Show("Zadejte platný počet hodin (celé číslo > 0).", "Chyba");
                return;
            }

            
            InvoiceItem newItem = new InvoiceItem
            {
                Description = selectedService.Description,
                HourlyRate = selectedService.HourlyRate,
                HoursWorked = hours
            };

            currentItems.Add(newItem);
            UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            
            decimal total = currentItems.Sum(item => item.TotalPrice);
            TotalAmountTextBlock.Text = $"Celková částka: {total:C}";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InvoiceNumberTextBox.Text) || DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Vyplňte číslo faktury a datum splatnosti.", "Chyba");
                return;
            }

            if (currentItems.Count == 0)
            {
                MessageBox.Show("Faktura musí obsahovat alespoň jednu položku.", "Chyba");
                return;
            }

            
            NewInvoice = new Invoice
            {
                InvoiceNumber = InvoiceNumberTextBox.Text,
                DueDate = DueDatePicker.SelectedDate.Value,
                Items = currentItems.ToList() 
            };

            

            this.DialogResult = true;
            this.Close();
        }
    }
}