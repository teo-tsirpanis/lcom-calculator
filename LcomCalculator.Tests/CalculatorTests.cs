using System;
using System.Reflection;
using LcomCalculator.Core;
using Mono.Cecil;
using Xunit;

namespace LcomCalculator.Tests
{
    public class CalculatorTests
    {
        [Theory]
        [InlineData(typeof(TestClasses.TestClass1), 2)]
        public void Test1(Type type, int expectedLcom)
        {
            using var asmDef = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location)!;
            Assert.NotNull(asmDef);

            var typeDef = asmDef.MainModule.GetType(type.FullName)!;
            Assert.NotNull(typeDef);

            var actualLcom = Calculator.CalculateLackOfCohesion(typeDef);
            Assert.Equal(expectedLcom, actualLcom);
        }
    }
}
