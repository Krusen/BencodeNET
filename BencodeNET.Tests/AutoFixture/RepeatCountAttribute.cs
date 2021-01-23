using System;
using System.Reflection;
using AutoFixture;
using AutoFixture.Xunit2;

namespace BencodeNET.Tests.AutoFixture
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class RepeatCountAttribute : CustomizeAttribute
    {
        private int Count { get; }

        public RepeatCountAttribute(int count)
        {
            Count = count;
        }

        public override ICustomization GetCustomization(ParameterInfo parameter)
        {
            return new RepeatCountCustomization(Count);
        }
    }
}
