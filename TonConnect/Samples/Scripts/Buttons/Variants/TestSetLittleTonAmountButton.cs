using System;

namespace UnitonConnect.Core.Demo
{
    public sealed class TestSetLittleTonAmountButton : TestSetTonAmountButton
    {
        private const double TARGET_TON_AMOUNT = 0.01f;

        public sealed override void Init()
        {
            SetAmount(Math.Round(TARGET_TON_AMOUNT, 2));
        }
    }
}