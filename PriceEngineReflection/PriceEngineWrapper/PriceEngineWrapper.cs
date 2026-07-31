using ACA.PriceEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PriceEngineWrapper
{
    public class PriceEngineWrapper
    {
        private PriceEngine _priceEngine = new PriceEngine();
        private MethodInfo _computeSubtotal;
        private MethodInfo _applyVolumeDiscount;
        private MethodInfo _applyLoyaltyDiscount;
        private MethodInfo _applyCoupon;
        private MethodInfo _applyVat;
        private MethodInfo _roundMoney;
        private MethodInfo _countUnits;

        public PriceEngineWrapper()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
            _computeSubtotal = typeof(PriceEngine).GetMethod("ComputeSubtotal", flags);
            _applyVolumeDiscount = typeof(PriceEngine).GetMethod("ApplyVolumeDiscount", flags);
            _applyLoyaltyDiscount = typeof(PriceEngine).GetMethod("ApplyLoyaltyDiscount", flags);
            _applyCoupon = typeof(PriceEngine).GetMethod("ApplyCoupon", flags);
            _applyVat = typeof(PriceEngine).GetMethod("ApplyVat", flags);
            _roundMoney = typeof(PriceEngine).GetMethod("RoundMoney", flags);
            _countUnits = typeof(PriceEngine).GetMethod("CountUnits", flags);
        }


        public decimal Calculate(PriceInput input)
        {            
            decimal amount = (decimal)_computeSubtotal.Invoke(_priceEngine, new object[] { input.Lines });
            amount = (decimal)_applyVolumeDiscount.Invoke(_priceEngine, new object[] { amount,
                _countUnits.Invoke(_priceEngine, new object[] { input.Lines } )});
            amount = (decimal)_applyLoyaltyDiscount.Invoke(_priceEngine, new object[] { amount, input.LoyaltyTier });
            amount = (decimal)_applyCoupon.Invoke(_priceEngine, new object[] { amount, input.CouponAmount });
            amount = (decimal)_applyVat.Invoke(_priceEngine, new object[] {amount, input.VatRate });

            return (decimal)_roundMoney.Invoke(_priceEngine, new object[] { amount });

        }
    }
}
