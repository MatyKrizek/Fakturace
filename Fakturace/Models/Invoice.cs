
using System.Collections.Generic;
using System.Linq;

namespace Fakturace.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string InvoiceNumber { get; set; }
        public System.DateTime DueDate { get; set; }

        
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        
        public decimal TotalAmount
        {
            get { return Items.Sum(i => i.TotalPrice); }
        }

        
        public override string ToString()
        {
            return $"{InvoiceNumber} ({TotalAmount:C})";
        }
    }
}