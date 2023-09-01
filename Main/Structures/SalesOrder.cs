using Main.Models;
using Microsoft.Build.Evaluation;

namespace Main.Structures
{
    public class SalesOrder
    {
        //public int SalesOrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public byte Status { get; set; }
        public int CustomerID { get; set; }
        public int? ShipToAddressID { get; set; }
        public int? BillToAddressID { get; set; }
        public int CreditCardID { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal TotalDue { get; set; }
        public string? Comment { get; set; }
        public ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();
    }
}