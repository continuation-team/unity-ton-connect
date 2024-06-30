using System;

namespace UnitonConnect.Core.Demo
{
    public class TestSetMediumTonAmountButton : TestSetTonAmountButton
    {
        private const double TARGET_TON_AMOUNT = 0.1f;

        public sealed override void Init()
        {
            SetAmount(Math.Round(TARGET_TON_AMOUNT, 1));
        }
    }
}