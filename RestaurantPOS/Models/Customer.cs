namespace RestaurantPOS.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string MembershipTier { get; set; } = "regular"; // 'regular', 'vip', 'vip_gold'
        public int LoyaltyPoints { get; set; }
    }
}
