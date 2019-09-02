using System;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;

namespace _PerformanceCounter
{
    class MyPerformanceCounter
    {
        static string _appName => "PerformanceCounter";

        static void Main()
        {
            SaveCountersInFileByAppName();
            GetInstanceAndCounterListByAppName();
        }

        public static void GetInstanceAndCounterListByAppName()
        {
            foreach (var cat in PerformanceCounterCategory.GetCategories())
                try
                {
                    string[] instances = cat.GetInstanceNames();

                    if (instances.Length > 0)
                    {
                        Console.WriteLine("Category Name: " + cat.CategoryName);
                        foreach (string instance in instances)
                            if (instance == _appName)
                            {
                                Console.WriteLine("  Instance: " + instance);
                                if (cat.InstanceExists(instance))
                                    foreach (PerformanceCounter ctr in cat.GetCounters(instance))
                                        Console.WriteLine("    Counter: " + ctr.CounterName + ", RawValue: " + ctr.RawValue + ", NextValue: " + ctr.NextValue());
                            }
                    }
                }
                catch { }
        }

        public static void SaveCountersInFileByAppName()
        {
            var x = new XElement("counters",
                from PerformanceCounterCategory cat in PerformanceCounterCategory.GetCategories()
                where cat.CategoryName.StartsWith(".NET")
                let instances = cat.GetInstanceNames()
                select new XElement("category", new XAttribute("name", cat.CategoryName),
                instances.Length == 0 ? from c in cat.GetCounters() select new XElement("counter", new XAttribute("name", c.CounterName))
                : from i in instances
                  where i == _appName
                  select new XElement("instance", new XAttribute("name", i),
                    !cat.InstanceExists(i) ? null :
                    from c in cat.GetCounters(i) select new XElement("counter", new XAttribute("name", c.CounterName), new XAttribute("rowValue", c.RawValue), new XAttribute("nextValue", c.NextValue())))));

            x.Save("counters.xml");
        }
    }
}
