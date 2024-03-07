using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    public class ClientVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public string Address { get; set; }
        public string Long { get; set; }
        public string Lati { get; set; }
        public string URL { get; set; }
        public bool Status { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Description { get; set; }
        public int ClientType { get; set; }
        public string Logo { get; set; }
        public string Background { get; set; }
        public bool CanOrder { get; set; }
        public int MaxService { get; set; }
        public int ServiceNum { get; set; }
        public int MainBranchId { get; set; }
        public int MaxBranch { get; set; }
        public int BranchNum { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedOn { get; set; }
        public ThemeVM? Theme { get; set; }
        public int? ThemeId { get; set; }
        public int MaxEmployee { get; set; }
        public int EmployeeNum { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsFoodicsClient { get; set; }
        public bool HasDelivery { get; set; }
        public bool IsTaxExcluded { get; set; }
        public string FoodicsBranchId { get; set; }
        public bool HasItemType { get; set; }
        public int CityId { get; set; }
        public string SecondBackground { get; set; }
        public string ThirdBackground { get; set; }
        public decimal DriverFee { get; set; }
        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string Twitter { get; set; }
        public string Youtube { get; set; }
        public string Snapchat { get; set; }
        public string Whatsapp { get; set; }
        public string Tiktok { get; set; }
        public string Saturday { get; set; }
        public string Sunday { get; set; }
        public string Monday { get; set; }
        public string Tuesday { get; set; }
        public string Wednesday { get; set; }
        public string Thursday { get; set; }
        public string Friday { get; set; }
        public string TaxNumber { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public int SupplierCode { get; set; }
        public bool HasPayment { get; set; }
        public string FoodicsPaymentMethodId { get; set; }
        public bool HasCash { get; set; }
        public bool HasCard { get; set; }
        public bool HasExternalImages { get; set; }
        public bool HasDeliveryCash { get; set; }
        public bool HasDeliveryCard { get; set; }
        public bool HasPickupCash { get; set; }
        public bool HasPickupCard { get; set; }
        public bool HasPOS { get; set; }
        public bool HasDeliveryPOS { get; set; }
        public bool HasPickupPOS { get; set; }
        public bool hasServiceFee { get; set; }
        public decimal ServiceFee { get; set; }
        public int MinDecimal { get; set; }
        public int MaxDecimal { get; set; }
        public int BackgroundVersion { get; set; }
        public int LogoVersion { get; set; }
        public int SecondBackgroundVersion { get; set; }
        public int ThirdBackgroundVersion { get; set; }
        public int SubscriptionId { get; set; }

        //public UserVM User { get; set; } = new UserVM();
        public bool ActivateLoyalty { get; set; }
        public decimal PointsToCurrency { get; set; }
        public decimal CurrencyToPoints { get; set; }
        public bool IsGalaxyClient { get; set; }
        public bool IsQuickPOSClient { get; set; }
        public int QuickPOSClientId { get; set; }
        public int QuickPOSBranchId { get; set; }
        public int QuickPOSProviderId { get; set; }
        public bool ActivateTableBooking { get; set; }
        public string MenuLanguage { get; set; }
        public bool ActivateSoundForCallWaiter { get; set; }
        public int Layout { get; set; }
        public bool HasLayout { get; set; }
        public decimal Radius { get; set; }
        public int MaxSMS { get; set; }
        public int MaxCount { get; set; }
        public bool HasChargeFee { get; set; }
        public decimal ChargeFee { get; set; }
        public decimal DeliveryDistance { get; set; }
        public bool HasActivatedDeleiveryDistance { get; set; }
        public bool hasDiningCharge { get; set; }
        public bool hasPickupCharge { get; set; }
        public bool hasServiceFeeDinein { get; set; }
        public bool hasServiceFeePickUp { get; set; }
        public List<ClientLabelsVM> Labels { get; set; } = new List<ClientLabelsVM>();

    }
}
