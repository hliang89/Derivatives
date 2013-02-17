using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Derivatives;

namespace Widget
{
    public enum BufferSize
    {
        Std = 100,
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SwapTest();
            }
            catch (Exception e)
            {
            }
        }

        static void SwapTest()
        {
            DateTime start = new DateTime(2004, 3, 29);
            DateTime maturity = new DateTime(2013, 3, 28);
            DateTime valuation = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            DateTime effectiveDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            double notional = 3300000;
            double swapRate = 0.03969;

            List<double> mat;
            Dictionary<double, double> libor;
            Dictionary<double, double> discRate;

            SwapType swapType = SwapType.PayFixedReceiveFloat;

            List<List<string>> parsedData = new List<List<string>>();

            using (StreamReader sr = new StreamReader("..\\..\\..\\Resources\\SwapData.csv"))
            {
                List<string> lineData = new List<string>();
                lineData = ProcessCSVLine(sr.ReadLine());
                parsedData.Add(lineData);
            }

            foreach (var lineData in parsedData)
            {
                foreach (var data in lineData)
                {
                    Console.Write(data + ", ");
                }
                Console.Write(Environment.NewLine);
            }
        }

        public static List<string> ProcessCSVLine(string line)
        {
            List<string> seperated = line.Split(',').ToList();

            List<string> cleaned = new List<string>();
            foreach (var s in seperated)
            {
                if (!String.IsNullOrWhiteSpace(s))
                {
                    cleaned.Add(s.Trim());
                }
            }
            return cleaned;
        }

        

    }

    
}
