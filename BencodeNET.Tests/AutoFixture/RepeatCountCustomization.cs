using AutoFixture;

namespace BencodeNET.Tests.AutoFixture
{
    public class RepeatCountCustomization : ICustomization
    {
        private int Count { get; }

        public RepeatCountCustomization(int count)
        {
            Count = count;
        }

        public void Customize(IFixture fixture)
        {
            fixture.RepeatCount = Count;
        }
    }
}
