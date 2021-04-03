// Copyright (c) 2021 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

using System;
using LcomCalculator.Core;
using Mono.Cecil;
using Xunit;

namespace LcomCalculator.Tests
{
    public class CalculatorTests
    {
        [Theory]
        [InlineData(typeof(TestClasses.TestClass1), 2)]
        // I had initially thought that a class with no fields should have an LCOM of zero,
        // but actually it shouldn't; because it does not have any state, its methods are not
        // cohesive at all; nothing stops the methods from being declared in different classes.
        [InlineData(typeof(Calculator), 1)]
        public void Test1(Type type, int expectedLcom)
        {
            using var asmDef = AssemblyDefinition.ReadAssembly(type.Assembly.Location)!;
            Assert.NotNull(asmDef);

            var typeDef = asmDef.MainModule.GetType(type.FullName)!;
            Assert.NotNull(typeDef);

            var actualLcom = Calculator.CalculateLackOfCohesion(typeDef);
            Assert.Equal(expectedLcom, actualLcom);
        }
    }
}
