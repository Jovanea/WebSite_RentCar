namespace eUseControl.BusinessLogic.Models
{
    public class ImageUpload
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}