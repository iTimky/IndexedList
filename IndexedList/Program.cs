using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IndexedList
{
    class Program
    {
        private static List<ManyToMany> list;
        private static IndexedList<ManyToMany> indexedList;
        private static List<ManyToMany> manyToManys;
        private static List<ManyToMany> resultsFromlist = new List<ManyToMany>();
        private static List<ManyToMany> resultsFromindexed = new List<ManyToMany>();


        private static int Count = 1000000;

        static void Main(string[] args)
        {
            Init();

//            var manyToMany1 = new ManyToMany(1, 1);
//            var manyToMany2 = new ManyToMany(1, 1);
//            var manyToMany3 = new ManyToMany(1, 1);
//            var toManys = new List<ManyToMany>();
//            toManys.Add(manyToMany1);
//            toManys.Add(manyToMany2);
//            toManys.Add(manyToMany3);
//
//            toManys.Remove(manyToMany1);
//            //var manyToMany4 = new ManyToMany(4, 4);
//            //var manyToMany5 = new ManyToMany(5, 5);
//            var hashIndexLeaf = new IndexLeaf<ManyToMany>(manyToMany1)
//                .Add(manyToMany2)
//                .Add(manyToMany3);
//                //.Add(manyToMany4)
//                //.Add(manyToMany5);
//
//            foreach (var toMany in hashIndexLeaf.Remove(manyToMany1).Value)
//            {
//                Console.WriteLine(toMany.FirstId);
//            }

            //indexLeaf = indexLeaf.Remove(manyToMany5);
//            var ints = new List<int>();
//            ints.Add(1);
//            ints.Add(1);
//            ints.Add(1);
//            ints.Add(1);
//            ints.Add(1);
//            ints.Add(1);
//            ints.Add(1);
//            ints.Add(1);
//            ints.Add(1);

            for (int i = 1; i <= Count; i *= 10)
                Test(i);

            //Test(100000);

            

            //IList<ManyToMany> toManys = new IndexedList<ManyToMany>() {new ManyToMany(1, 1)};
            //toManys.Where(i => i.FirstId == 1);

//            var indexedList = new IndexedList<ManyToMany>();
//            indexedList.AddIndex(mtm => mtm.FirstId);
//            indexedList.Add(manyToMany1);
//            indexedList.Add(manyToMany2);
//            indexedList.Add(manyToMany3);
//            
            //Console.WriteLine(indexedList.GetIndexes());
            Console.WriteLine("Done");
            Console.ReadKey();
        }



        static void Test(int count)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
                list = new List<ManyToMany>();
                list.AddRange(manyToManys.Take(count));
            stopwatch.Stop();
            Console.WriteLine("List init {0}: {1} ms", count, stopwatch.ElapsedMilliseconds);


            stopwatch.Reset();

            stopwatch.Start();
                indexedList = new IndexedList<ManyToMany>();
                indexedList.AddRange(manyToManys.Take(count));
                indexedList.AddIndex(mtm => mtm.FirstId);
            stopwatch.Stop();
            Console.WriteLine("IndexedList init {0}: {1} ms", count, stopwatch.ElapsedMilliseconds);


//            stopwatch.Reset();
//
//            stopwatch.Start();
//                indexedList.AddRange(manyToManys.Take(1000000));
//            stopwatch.Stop();
//            Console.WriteLine("IndexedList  add 1000000: {0} ms", stopwatch.ElapsedMilliseconds);

            
            stopwatch.Reset();


            stopwatch.Start();
            //foreach (var manyToMany in list)
            //    resultsFromlist.AddRange(list.Where(l => l.FirstId == manyToMany.FirstId).ToList());
                list.Where(l => l.FirstId == 1000).ToList();
            stopwatch.Stop();
            Console.WriteLine("List {0}: {1} ms", count, stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();

            stopwatch.Start();
//            foreach (var manyToMany in indexedList)
//                resultsFromlist.AddRange(indexedList.Where(l => l.FirstId == manyToMany.FirstId).ToList());
                indexedList.Where(l => l.FirstId == 1000).ToList();
            stopwatch.Stop();
            Console.WriteLine("IndexedList {0}: {1} ms", count, stopwatch.ElapsedMilliseconds);
            Console.WriteLine();

            list.Clear();
            indexedList.Clear();
        }


        static void Init()
        {
            list = new List<ManyToMany>(Count);
            indexedList = new IndexedList<ManyToMany>();
            //indexedList.AddIndex(mtm => mtm.FirstId);

            manyToManys = new List<ManyToMany>();
            var manyToMany = new ManyToMany(1, 1);
            for (int i = 0; i < Count; i++)
                //manyToManys.Add(manyToMany);
                manyToManys.Add(new ManyToMany(i, Count - i));
        }


        class ManyToMany
        {
            public ManyToMany(int firstId, int secondId)
            {
                FirstId = firstId;
                SecondId = secondId;
            }

            public int FirstId { get; set; }
            public int SecondId { get; set; }

            public override string ToString()
            {
                return string.Format("FirstId: {0}, SecondId: {1}", FirstId, SecondId);
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                var manyToMany = obj as ManyToMany;
                if (manyToMany == null)
                    return false;
                
                if (manyToMany.FirstId == FirstId)
                    return true;

                return base.Equals(obj);
            }
        }
    }
}
