namespace Main.Structures
{
    public class UpdateProduct
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Category { get; set; }
        public decimal ListPrice { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; }
        public decimal? Weight { get; set; }
        public string? ThumbNailPhoto { get; set; }
        public string? ThumbnailPhotoFileName { get; set; }
    }
}
