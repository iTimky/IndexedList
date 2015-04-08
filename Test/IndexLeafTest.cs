using IndexedList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class IndexLeafTest
    {
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
        }


        [TestMethod]
        public void TestLeafCreateEmpty()
        {
            var indexLeaf = new LastIndexLeaf<TestObject>();
            Assert.AreEqual(indexLeaf.Count, 0);
            Assert.IsTrue(indexLeaf.Item == null);
        }

        [TestMethod]
        public void TestLeafCreate()
        {
            var testObject = new TestObject();
            var indexLeaf = new LastIndexLeaf<TestObject>(testObject);
            Assert.IsTrue(indexLeaf.Contains(testObject));
            Assert.AreEqual(1, indexLeaf.Count);
        }

        [TestMethod]

        public void TestLeafAdd()
        {
            var obj1 = new TestObject(1, null);
            var obj2 = new TestObject(2, null);
            var indexLeaf = new LastIndexLeaf<TestObject>(obj1);
            indexLeaf = indexLeaf.Add(obj2);

            Assert.IsTrue(indexLeaf.Contains(obj1));
            Assert.IsTrue(indexLeaf.Contains(obj2));
            Assert.AreEqual(2, indexLeaf.Count);
        }


        [TestMethod]
        public void TestLeafDeleteFirst()
        {
            var obj1 = new TestObject(1, null);
            var obj2 = new TestObject(2, null);
            var obj3 = new TestObject(3, null);
            var indexLeaf = new LastIndexLeaf<TestObject>(obj1);
            indexLeaf = indexLeaf.Add(obj2).Add(obj3)
                        .Remove(obj1).Value;

            Assert.IsFalse(indexLeaf.Contains(obj1));
            Assert.IsTrue(indexLeaf.Contains(obj2));
            Assert.IsTrue(indexLeaf.Contains(obj3));
            Assert.AreEqual(2, indexLeaf.Count);
        }

        [TestMethod]
        public void TestLeafDeleteMiddle()
        {
            var obj1 = new TestObject(1, null);
            var obj2 = new TestObject(2, null);
            var obj3 = new TestObject(3, null);
            var indexLeaf = new LastIndexLeaf<TestObject>(obj1);
            indexLeaf = indexLeaf.Add(obj2).Add(obj3)
                        .Remove(obj2).Value;

            Assert.IsFalse(indexLeaf.Contains(obj2));
            Assert.IsTrue(indexLeaf.Contains(obj1));
            Assert.IsTrue(indexLeaf.Contains(obj3));
            Assert.AreEqual(2, indexLeaf.Count);
        }

        [TestMethod]
        public void TestLeafDeleteLast()
        {
            var obj1 = new TestObject(1, null);
            var obj2 = new TestObject(2, null);
            var obj3 = new TestObject(3, null);
            var indexLeaf = new LastIndexLeaf<TestObject>(obj1);
            indexLeaf = indexLeaf.Add(obj2).Add(obj3)
                        .Remove(obj3).Value;

            Assert.IsFalse(indexLeaf.Contains(obj3));
            Assert.IsTrue(indexLeaf.Contains(obj1));
            Assert.IsTrue(indexLeaf.Contains(obj2));
            Assert.AreEqual(2, indexLeaf.Count);
        }
    }
}
