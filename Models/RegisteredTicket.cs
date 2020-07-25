using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public class RegisteredTicket
    {
        public bool IsComplete { get; set; }
        public string PaidUserID { get; set; }
        public List<ReservationSeat> Seats { get; set; }
        public PaymentType Paytype { get; set; }
        public int Cost { get; set; }
    }

    public class PaymentType
    {
        public bool IsCard { get; set; }
        public bool IsAcquirer { get; set; }
        public bool IsPrepaid { get; set; }
        public bool IsPostpay { get; set; }
        public bool IsCash { get; set; }
        public CardType Bland { get; set; }
        public AcquirerType Acquire { get; set; }
        public PrepaidType Prepaid { get; set; }
    }

    public enum CardType 
    {
        Master,
        VISA,
        JCB,
        AMEX,
        DINERS
    }
    public enum AcquirerType
    {
        Rakuten,
        ApplePay,
        LINEPay
    }
    public enum PrepaidType
    {
        MovieTicket,
        IONGift,
        TOHOGift,
        Cinemilage
    }
    public enum PostpayType
    {
        AU,
        Docomo,
        Softbank
    }
}
