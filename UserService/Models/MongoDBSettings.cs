namespace Models
{
    public class MongoDBSettings
    {
        public string ConnectionAuctionDB { get; set; } = null!;
        public string DatabaseName { get; set; } = "UserDB";
        public string CollectionName { get; set; } = "UserCollection";
    }
}