namespace UnitonConnect.Core.Demo
{
    public sealed class TestSetBigTonAmountButton : TestSetTonAmountButton
    {
        private const double TARGET_TON_AMOUNT = 1f;

        public sealed override void Init()
        {
            SetAmount(TARGET_TON_AMOUNT);
        }
    }
}