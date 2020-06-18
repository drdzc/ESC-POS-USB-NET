using System;
using System.Collections.Generic;
using System.Text;

namespace TestPrinterApp
{
    class PurchaseOrderSingleItem
    {
        public int id { get; set; }
        public string employee { get; set; }
        public string client { get; set; }
        public string total { get; set; }
        public int type { get; set; }
        public DateTime createdDt { get; set; }
        public int isClosed { get; set; }
        public string cashPaidTotal { get; set; }
        public string creditCardTotal { get; set; }
        public string debitCardTotal { get; set; }
        public string checkTotal { get; set; }
        public string transferTotal { get; set; }
        public string changeTotal { get; set; }
        public List<PurchaseOrderProduct> products { get; set; }
    }
}
