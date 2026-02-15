namespace LabCollect.Models
{
    public class LaboratoryTestModel
    {
        public int? TestId { get; set; }
        public string TestName { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? NormalRange { get; set; }
        public string? Unit { get; set; }
        public decimal Price { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
