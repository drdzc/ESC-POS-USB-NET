namespace TestPrinterApp
{
    class PurchaseOrderProduct
    {
        public int id { get; set; }
        public string name { get; set; }
        public int quantity { get; set; }
        public string costStr { get; set; }
        public float calculatedDiscount { get; set; }
        public int isGift { get; set; }
        public string costWithDiscountStr { get; set; }
        public string model { get; set; }
        public string baseCostStr { get; set; }

        public string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
        }
    }
}
