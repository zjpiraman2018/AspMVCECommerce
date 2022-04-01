namespace AspMVCECommerce.ViewModel
{
    public class ImageViewModel
    {
        public int? ImageId { get; set; }
        public string Title { get; set; }
        public System.Web.HttpPostedFileWrapper ImageFile { get; set; }
        public int? ProductId { get; set; }
        public bool? Default { get; set; }
        public string ImagePath { get; set; }
        public bool? IsRemove { get; set; }
    }
}