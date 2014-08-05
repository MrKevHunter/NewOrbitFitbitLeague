namespace neworbitfitbitleague.Controllers
{
    public class MovesAccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public long user_id { get; set; }
    }
}