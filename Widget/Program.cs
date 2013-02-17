using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            int swapType = 1;

            


        }

    }

    
}
