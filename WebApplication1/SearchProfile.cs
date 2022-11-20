namespace WebApplication1
{
    public class SearchProfile
    {
        private static int count=0;
        public int Id { get; set; }
        public string profile;

        public SearchProfile(string profile)
        {
            count++;
            this.profile = profile;
            this.Id = count;
        }
    }
}
