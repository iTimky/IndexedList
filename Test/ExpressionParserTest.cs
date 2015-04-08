using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using IndexedList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class ExpressionParserTest
    {
        int A { get; set; }
        class TestObject
        {
            public TestObject() { }

            public TestObject(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id;
            public string Name { get; set; }
            public TestObject Child { get; set; }
            public Guid Guid;
            public Guid? NullableGuid { get; set; }
            public TestValue? TestValue { get; set; }
        }

        class DeriviedTestObject : TestObject { }

        class GenericTestObject<T>
        {
            public T Prop { get; private set; }
        }

        struct TestValue
        {
            public int A;
        }

        class DeriviedGenericTestObject : GenericTestObject<int> { }

        [TestMethod]
        public void TestGetConstName()
        {
            Assert.AreEqual(null, ExpressionParser.GetMemberName((TestObject o) => 1));
        }

        [TestMethod]
        public void TestGetFieldName()
        {
            Assert.AreEqual("Id", ExpressionParser.GetMemberName((TestObject o) => o.Id));
        }

        [TestMethod]
        public void TestGetPropertyName()
        {
            Assert.AreEqual("Name", ExpressionParser.GetMemberName((TestObject o) => o.Name));
        }


        [TestMethod]
        public void TestGetConstHashValue()
        {
            Assert.AreEqual(3.GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.Id == 3));
        }


        [TestMethod]
        public void TestGetFieldHashValue()
        {
            var testObject = new TestObject(1, null);
            Assert.AreEqual(1.GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.Id == testObject.Id));
        }

        [TestMethod]
        public void TestGetPropHashValue()
        {
            var testObject = new TestObject(1, "SomeName");
            Assert.AreEqual("SomeName".GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.Name == testObject.Name));
        }

        [TestMethod]
        public void TestGetNestedHashValue()
        {
            var child = new TestObject(1, "SomeName");
            var testObject = new TestObject() {Child = child};
            Assert.AreEqual(1.GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.Id == testObject.Child.Id));
        }


        [TestMethod]
        public void TestGetDerivedHashValue()
        {
            var deriviedTestObject = new DeriviedTestObject(){Id = 1};
            Assert.AreEqual(1.GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.Id == deriviedTestObject.Id));
        }

        [TestMethod]
        public void TestGetNewObjHashValue()
        {
            Assert.AreEqual(1.GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.Id == new TestObject(1, "SomeName").Id));
        }

        [TestMethod]
        public void TestGetMemberInitHashValue()
        {
            var child = new TestObject(1, "SomeName");
            Assert.AreEqual(1.GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.Id == new TestObject { Child = child }.Child.Id));
        }

        [TestMethod]
        public void TestNewPrimitiveHashValue()
        {
            Assert.AreEqual(172104656.GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.Guid == new Guid("68F45EDD-24B6-4589-A8B9-46B8A82DD884")));
        }

        [TestMethod]
        public void TestGetPrivatePropertyHashValue()
        {
            Assert.AreEqual(0.GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.Id == A));
        }

        [TestMethod]
        public void TestGetNullableHashValue()
        {
            var testObject = new TestObject(){NullableGuid = new Guid("68F45EDD-24B6-4589-A8B9-46B8A82DD884")};
            Assert.AreEqual(172104656.GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.Guid == testObject.NullableGuid.Value));
        }

        [TestMethod]
        public void TestGetTestedNullableHashValue()
        {
            var testValue = new TestValue() {A = 1};
            var testObject = new TestObject() { TestValue = testValue };
            Assert.AreEqual(1.GetHashCode(), ExpressionParser.GetMemberHash((TestObject o) => o.TestValue.Value.A == testObject.TestValue.Value.A));
        }
    }
}
