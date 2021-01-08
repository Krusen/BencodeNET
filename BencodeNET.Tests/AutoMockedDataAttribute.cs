using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Xunit;

namespace BencodeNET.Tests
{
    public class AutoMockedDataAttribute : CompositeDataAttribute
    {
        public AutoMockedDataAttribute()
            : this(new BaseAutoMockedDataAttribute())
        { }

        public AutoMockedDataAttribute(params object[] values)
            : this(new BaseAutoMockedDataAttribute(), values)
        { }

        private AutoMockedDataAttribute(BaseAutoMockedDataAttribute baseAutoDataAttribute, params object[] values)
            : base(new InlineDataAttribute(values), baseAutoDataAttribute)
        { }

        private class BaseAutoMockedDataAttribute : AutoDataAttribute
        {
            public BaseAutoMockedDataAttribute()
                : base(Configure)
            {
            }

            private static IFixture Configure()
            {
                return new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
            }
        }
    }
}
