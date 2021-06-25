using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Nop.Core.Domain.Orders
{
    public enum MarketingChannel
    {
        
        Online = 1,
        SDS = 2,
        PPC=3,
        Radio_92_3=4,
        AthensBus=5,
        Billboard=6,
        EventSponsorship=7,
        TV=8,
        AthensNewspaperAd=9,
        OconeeNewspaperAd=10,
        PromotionalItem=11,
        WordofMouth=12,
        BusinessCard=13,
        ChamberofCommerce=14,
        FacetoFace=15

    }

    public enum PaymentType {
        [Description("Paypal")]
        Paypal=1,
        [Description("Check money order")]
        CheckMoneyOrder =2,
        [Description("Cash")]
        Cash =3
    }
}
