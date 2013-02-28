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
            DateTime valuation = new DateTime(2005, 8, 14);
            DateTime effectiveDate = new DateTime(2004, 3, 29);
            double notional = 3300000;
            double swapRate = 0.03969;

            List<double> mat = new List<double>();
            Dictionary<double, double> libor = new Dictionary<double, double>();
            Dictionary<double, double> discRate = new Dictionary<double,double>();

            double yr = 0;
            double rate = 0;
            SwapType swapType = SwapType.PayFixedReceiveFloat;

            List<List<string>> parsedData = new List<List<string>>();

            using (StreamReader sr = new StreamReader("..\\..\\..\\Resources\\SwapData.csv"))
            {
                List<string> lineData = new List<string>();

                while (!sr.EndOfStream)
                {
                    lineData = ProcessCSVLine(sr.ReadLine());
                    parsedData.Add(lineData);
                }
            }


            mat.Add(0);
            libor.Add(0, 0);
            discRate.Add(0, 0);
            foreach (var lineData in parsedData)
            {
                
                if (lineData[0].Contains("MO"))
                {
                    yr = Double.Parse(lineData[0].Substring(0, lineData[0].IndexOf(' '))) / 12;
                }
                else if (lineData[0].Contains("WK"))
                {
                    yr = Double.Parse(lineData[0].Substring(0, lineData[0].IndexOf(' '))) / 52;
                }
                else if (lineData[0].Contains("DY"))
                {
                    yr = Double.Parse(lineData[0].Substring(0, lineData[0].IndexOf(' '))) / 365;
                }
                else
                {
                    yr = Double.Parse(lineData[0].Substring(0, lineData[0].IndexOf(' ')));
                }
                

                mat.Add(yr);
                libor.Add(yr, Double.Parse(lineData[1]));
                discRate.Add(yr, Double.Parse(lineData[2]));
            }
            

            Swap s = new Swap(notional, maturity, effectiveDate, start, start.AddDays(1), libor, discRate, swapRate, SwapType.PayFixedReceiveFloat);

            //foreach (var lineData in parsedData)
            //{
            //    foreach (var data in lineData)
            //    {
            //        Console.Write(data + ", ");
            //    }
            //    Console.Write(Environment.NewLine);
            //}
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
