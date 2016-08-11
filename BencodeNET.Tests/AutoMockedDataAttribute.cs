using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace BencodeNET.Tests
{
    public class AutoMockedDataAttribute : AutoDataAttribute
    {
        public AutoMockedDataAttribute()
            : base(new Fixture().Customize(new AutoConfiguredNSubstituteCustomization()))
        { }
    }

    public class InlineAutoMockedDataAttribute : CompositeDataAttribute
    {
        public InlineAutoMockedDataAttribute()
            : this(new AutoMockedDataAttribute())
        { }

        public InlineAutoMockedDataAttribute(params object[] values)
            : this(new AutoMockedDataAttribute(), values)
        { }

        public InlineAutoMockedDataAttribute(AutoMockedDataAttribute autoDataAttributeAttribute, params object[] values)
            : base(new InlineDataAttribute(values), autoDataAttributeAttribute)
        { }
    }
}
