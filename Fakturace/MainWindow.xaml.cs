
using Fakturace.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Fakturace
{
    public partial class MainWindow : Window
    {
       
        private List<Customer> allCustomers = new List<Customer>();
        private List<Invoice> allInvoices = new List<Invoice>();

        
        private const string SUPPLIER_NAME = "Maty s.r.o.";
        private const string SUPPLIER_ADDRESS = "Bažina 8, 569 38 Springfield";
        private const string SUPPLIER_ICO = "669977";
        private const string SUPPLIER_DIC = "CZ669977";
        private const string SUPPLIER_BANK = "20001-123454321/0800";

        
        private string customersFile = "customers.json";
        private string invoicesFile = "invoices.json";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            RefreshCustomerList();
        }

        

        private void LoadData()
        {
            try
            {
                if (File.Exists(customersFile))
                {
                    string json = File.ReadAllText(customersFile);
                    allCustomers = JsonConvert.DeserializeObject<List<Customer>>(json) ?? new List<Customer>();
                }
                if (File.Exists(invoicesFile))
                {
                    string json = File.ReadAllText(invoicesFile);
                    allInvoices = JsonConvert.DeserializeObject<List<Invoice>>(json) ?? new List<Invoice>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání dat: {ex.Message}", "Chyba");
            }
        }

        private void SaveData()
        {
            try
            {
                string customersJson = JsonConvert.SerializeObject(allCustomers, Formatting.Indented);
                File.WriteAllText(customersFile, customersJson);

                string invoicesJson = JsonConvert.SerializeObject(allInvoices, Formatting.Indented);
                File.WriteAllText(invoicesFile, invoicesJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při ukládání dat: {ex.Message}", "Chyba");
            }
        }

        

        private void RefreshCustomerList()
        {
            CustomerListBox.ItemsSource = null;
            CustomerListBox.ItemsSource = allCustomers;
            CustomerListBox.DisplayMemberPath = "Name";
        }

        private void RefreshInvoiceList(int customerId)
        {
            var filteredInvoices = allInvoices
                .Where(inv => inv.CustomerId == customerId)
                .ToList();

            InvoiceListBox.ItemsSource = null;
            InvoiceListBox.ItemsSource = filteredInvoices;
            InvoiceListBox.DisplayMemberPath = "InvoiceNumber";
        }

        

        private void CustomerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Customer selectedCustomer = (Customer)CustomerListBox.SelectedItem;

            if (selectedCustomer != null)
            {
                RefreshInvoiceList(selectedCustomer.Id);
                
                AddInvoiceButton.IsEnabled = true;
                DeleteCustomerButton.IsEnabled = true; 
            }
            else
            {
                
                InvoiceListBox.ItemsSource = null;
                AddInvoiceButton.IsEnabled = false;
                DeleteCustomerButton.IsEnabled = false; 
                GenerateInvoiceButton.IsEnabled = false;
                DeleteInvoiceButton.IsEnabled = false;
            }
        }

        
        private void InvoiceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Invoice selectedInvoice = (Invoice)InvoiceListBox.SelectedItem;

            if (selectedInvoice != null)
            {
                
                GenerateInvoiceButton.IsEnabled = true;
                DeleteInvoiceButton.IsEnabled = true;
            }
            else
            {
                
                GenerateInvoiceButton.IsEnabled = false;
                DeleteInvoiceButton.IsEnabled = false;
            }
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            AddCustomerWindow addWindow = new AddCustomerWindow();
            if (addWindow.ShowDialog() == true)
            {
                Customer customer = addWindow.NewCustomer;
                customer.Id = (allCustomers.Any() ? allCustomers.Max(c => c.Id) : 0) + 1;
                allCustomers.Add(customer);
                SaveData();
                RefreshCustomerList();
            }
        }

        private void AddInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            Customer selectedCustomer = (Customer)CustomerListBox.SelectedItem;
            if (selectedCustomer == null) return;

            AddInvoiceWindow addInvoiceWin = new AddInvoiceWindow();
            if (addInvoiceWin.ShowDialog() == true)
            {
                Invoice newInvoice = addInvoiceWin.NewInvoice;
                newInvoice.Id = (allInvoices.Any() ? allInvoices.Max(i => i.Id) : 0) + 1;
                newInvoice.CustomerId = selectedCustomer.Id;
                allInvoices.Add(newInvoice);
                SaveData();
                RefreshInvoiceList(selectedCustomer.Id);
            }
        }

        

        private void GenerateInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            

            Customer selectedCustomer = (Customer)CustomerListBox.SelectedItem;
            Invoice selectedInvoice = (Invoice)InvoiceListBox.SelectedItem;

            if (selectedCustomer == null || selectedInvoice == null)
            {
                MessageBox.Show("Musíte vybrat zákazníka i fakturu.", "Chyba");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = $"Faktura_{selectedInvoice.InvoiceNumber}.txt";
            saveDialog.Filter = "Textový soubor (*.txt)|*.txt";

            if (saveDialog.ShowDialog() == true)
            {
                string fileContent = "--- FAKTURA ---\n\n";
                fileContent += $"Číslo faktury: {selectedInvoice.InvoiceNumber}\n";
                fileContent += $"Datum splatnosti: {selectedInvoice.DueDate.ToShortDateString()}\n";
                fileContent += $"Datum vystavení: {DateTime.Today.ToShortDateString()}\n"; 

                
                fileContent += "\n--- DODAVATEL ---\n";
                fileContent += $"Název: {SUPPLIER_NAME}\n";
                fileContent += $"Adresa: {SUPPLIER_ADDRESS}\n";
                fileContent += $"IČO: {SUPPLIER_ICO}\n";
                
                if (!string.IsNullOrEmpty(SUPPLIER_DIC))
                {
                    fileContent += $"DIČ: {SUPPLIER_DIC}\n";
                }
                fileContent += $"Bankovní spojení: {SUPPLIER_BANK}\n";

                fileContent += "\n--- ODBĚRATEL ---\n";
                fileContent += $"Jméno: {selectedCustomer.Name}\n";
                fileContent += $"IČO: {selectedCustomer.ICO}\n";
                fileContent += $"Adresa: {selectedCustomer.Address}\n";

                fileContent += "\n--- POLOŽKY FAKTURY ---\n";

                foreach (var item in selectedInvoice.Items)
                {
                    fileContent += $"- {item.Description}: {item.HoursWorked} hod. x {item.HourlyRate:C} = {item.TotalPrice:C}\n";
                }

                fileContent += $"\nCELKEM K ÚHRADĚ: {selectedInvoice.TotalAmount:C}\n";

                fileContent += "\nSplaceno do data splatnosti. Děkujeme!\n"; 

                try
                {
                    File.WriteAllText(saveDialog.FileName, fileContent);
                    MessageBox.Show($"Faktura byla úspěšně uložena do:\n{saveDialog.FileName}", "Hotovo");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Došlo k chybě při ukládání: {ex.Message}", "Chyba");
                }
            }
        }

        

        private void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            Customer selectedCustomer = (Customer)CustomerListBox.SelectedItem;
            if (selectedCustomer == null) return;

            
            MessageBoxResult result = MessageBox.Show(
                $"Opravdu chcete smazat odběratele '{selectedCustomer.Name}'?\nSmažou se tím i VŠECHNY jeho faktury!",
                "Potvrzení smazání",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                
                allInvoices.RemoveAll(invoice => invoice.CustomerId == selectedCustomer.Id);

                
                allCustomers.Remove(selectedCustomer);

                
                SaveData();

                
                RefreshCustomerList();
            }
        }

        private void DeleteInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            Invoice selectedInvoice = (Invoice)InvoiceListBox.SelectedItem;
            Customer selectedCustomer = (Customer)CustomerListBox.SelectedItem;
            if (selectedInvoice == null || selectedCustomer == null) return;

            
            MessageBoxResult result = MessageBox.Show(
                $"Opravdu chcete smazat fakturu '{selectedInvoice.InvoiceNumber}'?",
                "Potvrzení smazání",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                
                allInvoices.Remove(selectedInvoice);

                
                SaveData();

                
                RefreshInvoiceList(selectedCustomer.Id);
            }
        }
    }
}