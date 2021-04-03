// Copyright (c) 2021 Theodore Tsirpanis
//
// This software is released under the MIT License.
// https://opensource.org/licenses/MIT

namespace LcomCalculator.Tests.TestClasses
{
    public abstract class TestClass1 {
        private static int f1;
        private int f2 { get; set; }
        private int f3, f4;

        public int M1() => f1 + f2;

        private void M2(int x) => f3 = x;

        internal void M3() => f3 = f4;

        public static ref int M4() => ref f1;

        // The following shouldn't affect the metric's calculation:
        public TestClass1(string x) => f2 = int.Parse(x);

        public abstract void ShouldBeIgnored();
    }
}
