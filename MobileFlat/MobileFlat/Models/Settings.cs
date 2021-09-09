namespace MobileFlat.Models
{
    public class Settings
    {
        public string MosOblEircUser { get; set; }
        public string MosOblEircPassword { get; set; }
        public string GlobusUser { get; set; }
        public string GlobusPassword { get; set; }

        public bool IsSet
        {
            get =>
                !string.IsNullOrWhiteSpace(MosOblEircUser) &&
                !string.IsNullOrWhiteSpace(MosOblEircPassword) &&
                !string.IsNullOrWhiteSpace(GlobusUser) &&
                !string.IsNullOrWhiteSpace(GlobusPassword);
        }
    }
}
