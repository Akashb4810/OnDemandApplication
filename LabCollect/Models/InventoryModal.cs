namespace LabCollect.Models
{
    // Models/Inventory/Reagent.cs
    //public class Reagent
    //{
    //    public int ReagentId { get; set; }
    //    public string ReagentName { get; set; }
    //    public string Unit { get; set; }
    //    public decimal? AlertThreshold { get; set; }
    //    public decimal? LowStockLimit { get; set; }
    //}

    //// Models/Inventory/ReagentLot.cs
    //public class ReagentLot
    //{
    //    public int LotId { get; set; }
    //    public int ReagentId { get; set; }
    //    public int TestsPerUnit { get; set; }
    //    public int TestsDone { get; set; }
    //    public string LotNumber { get; set; }
    //    public string BatchNo { get; set; }
    //    public decimal OriginalQuantity { get; set; }
    //    public decimal Quantity { get; set; }
    //    public DateTime? ExpiryDate { get; set; }
    //    public DateTime ReceivedDate { get; set; }
    //    public string ReagentName { get; set; } // joined helper
    //}

    //// Models/Inventory/TestReagentUsageModel.cs
    public class TestReagentUsageModel
    {
        public int UsageId { get; set; }
        public int TestId { get; set; }
        public int ReagentId { get; set; }
        public decimal QuantityPerTest { get; set; }
        public string ReagentName { get; set; }
        public string TestName { get; set; }
    }
    public class AddTestReagentUsageModel
    {
        public int TestId { get; set; }

        public List<ReagentUsageItem> Reagents { get; set; } = new();
    }

    public class ReagentUsageItem
    {
        public int ReagentId { get; set; }
        public decimal QuantityPerTest { get; set; }
        public string ReagentName { get; set; }
    }

    //public class ReagentInventoryViewModel
    //{
    //    public int ReagentId { get; set; }
    //    public string ReagentName { get; set; }
    //    public string Unit { get; set; }
    //    public decimal ReorderLevel { get; set; }
    //    public decimal CurrentStock { get; set; }
    //    public bool IsLowStock { get; set; }
    //}

    //public class ReagentStockViewModel
    //{
    //    public Reagent Reagent { get; set; }
    //    public decimal TotalQty { get; set; }
    //    public bool IsLow { get; set; }
    //}

    //public class ReagentLotViewModel
    //{
    //    public int LotId { get; set; }
    //    public int ReagentId { get; set; }

    //    public string ReagentName { get; set; }

    //    // Lot details
    //    public string LotNumber { get; set; }
    //    public DateTime ExpiryDate { get; set; }

    //    // Quantity in this lot
    //    public decimal Quantity { get; set; }

    //    // User manually enters how many tests can be done per quantity
    //    public int TestsPerUnit { get; set; }

    //    // How many tests have been done from this lot
    //    public int TestsDone { get; set; }

    //    // Auto-calculated remaining tests
    //    public int RemainingTests
    //    {
    //        get
    //        {
    //            return (int)(Quantity * TestsPerUnit) - TestsDone;
    //        }
    //    }
    //}


    //public class ReagentIndexViewModel
    //{
    //    public int ReagentId { get; set; }

    //    public string ReagentName { get; set; }

    //    public string Unit { get; set; }

    //    // Total quantity available (sum of all lots or main master qty)
    //    public decimal TotalQuantity { get; set; }

    //    // Minimum threshold set in reagent master
    //    public decimal LowLevel { get; set; }

    //    // Show red alert UI in index
    //    public bool IsLow
    //    {
    //        get
    //        {
    //            return TotalQuantity <= LowLevel;
    //        }
    //    }

    //    // For showing number of lots available under this reagent
    //    public int TotalLots { get; set; }

    //    // For showing total tests possible across all lots
    //    public int TotalTests { get; set; }

    //    // Total tests remaining across all lots
    //    public int RemainingTests { get; set; }
    //}
    // Models/Reagent.cs
    public class Reagent
    {
        public int ReagentId { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal? LowStockLimit { get; set; }
    }

    // Models/ReagentLot.cs
    public class ReagentLot
    {
        public int LotId { get; set; }
        public int ReagentId { get; set; }
        public string LotNumber { get; set; }
        public decimal OriginalQuantity { get; set; }
        public decimal Quantity { get; set; }
        public int TestsPerUnit { get; set; }
        public int TestsDone { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime ReceivedDate { get; set; }

        // For UI convenience
        public string ReagentName { get; set; }
        public int TotalTests => (int)(OriginalQuantity * TestsPerUnit);
        public int RemainingTests => TotalTests - TestsDone;
    }

    // Models/ReagentIndexViewModel.cs
    public class ReagentIndexViewModel
    {
        public int ReagentId { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal TotalQty { get; set; }
        public int TotalTestsDone { get; set; }
        public decimal LowStockLimit { get; set; }
        public bool IsLow => TotalQty <= LowStockLimit;
    }
    public class TestReagentUsageViewModel
    {
        public int UsageId { get; set; }
        public int TestId { get; set; }
        public string TestName { get; set; }
        public int ReagentId { get; set; }
        public string ReagentName { get; set; }
        public decimal QuantityPerTest { get; set; }
        public string Unit { get; set; }
        public decimal LowStockLimit { get; set; }
    }

    public class ReagentsLot
    {
        public int LotId { get; set; }
        public string LotNumber { get; set; }
        public DateTime ReceivedDate { get; set; }

        // Child list
        public List<ReagentLotDetail> Details { get; set; } = new List<ReagentLotDetail>();
    }
    public class ReagentLotDetail
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public int ReagentId { get; set; }

        public decimal Quantity { get; set; }
        public decimal OriginalQuantity { get; set; }
        public int TestsPerUnit { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

}
