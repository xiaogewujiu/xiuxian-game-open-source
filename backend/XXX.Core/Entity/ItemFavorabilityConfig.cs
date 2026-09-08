namespace XXX.Entity
{
    public class ItemFavorabilityGiftConfig
    {
        public int FavorabilityValue { get; set; }
        public int DailyLimit { get; set; } = 5;
        public bool CanGift { get; set; } = true;
    }
}
