

namespace Fakturace.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }
        public string Description { get; set; } 
        public decimal HourlyRate { get; set; } 
        public int HoursWorked { get; set; } 

        
        public decimal TotalPrice
        {
            get { return HourlyRate * HoursWorked; }
        }

        
        public override string ToString()
        {
            return $"{Description} ({HoursWorked} hod. x {HourlyRate:C} = {TotalPrice:C})";
        }
    }
}