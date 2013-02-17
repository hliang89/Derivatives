using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRUtilities.QRDerivatives
{
    public class SwapSetup
    {
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public DateTime ValuationDate { get; set; }
        public DateTime EffectiveDate { get; set; }

        public double Notional { get; set; }
        public double SwapRatea { get; set; }

    }
}
