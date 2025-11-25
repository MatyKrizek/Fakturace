
using Fakturace.Models;
using System.Windows;

namespace Fakturace
{
    public partial class AddCustomerWindow : Window
    {
        
        public Customer NewCustomer { get; private set; }

        public AddCustomerWindow()
        {
            InitializeComponent();
            NameTextBox.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Jméno nesmí být prázdné.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewCustomer = new Customer
            {
                Name = NameTextBox.Text,
                ICO = IcoTextBox.Text,
                Address = AddressTextBox.Text
            };

            this.DialogResult = true;
            this.Close();
        }
    }
}