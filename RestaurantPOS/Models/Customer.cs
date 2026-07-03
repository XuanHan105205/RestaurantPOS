namespace RestaurantPOS.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string MembershipTier { get; set; } // 'regular', 'vip', 'vip_gold'
        public int LoyaltyPoints { get; set; }
    }
}
