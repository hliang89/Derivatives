using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Derivatives
{
    public enum SwapType
    {
        PayFloatReceiveFixed = 0,
        PayFixedReceiveFloat = 1
    }

    public static class SwapConstants
    {
        public const int NumDates = 100;
        public const int Thirty = 30;
        public const int Three_Sixty = 360;
        public const int Three_Sixty_Five = 365;
        public const int Notional = 100000;

        public static List<DateTime> PayDates = new List<DateTime>();

        public static double Interpolate(double rate1, double rate2, double t1, double t2, double x)
        {
            double dy = rate2 - rate1;
            double dx = t2 - t1;
            double slope = dy / dx;
            return rate1 + slope * x;
        }
    }

    class FloatingLeg
    {
        double floatlegBasis;
        List<double> floatRates;
        Dictionary<double, double> floatLegRate;
        Dictionary<double, double> payfloatLeg;
        DateTime startDate;
        DateTime maturityDate;
        DateTime valuationDate;
        double value;
        double spread;
        double notional;
        double duration;
        double accrual;
        int payFrequency;

        internal Dictionary<double, double> FloatLegRate { get { return floatLegRate; } set { floatLegRate = value; } }
        internal DateTime EffectiveDate { get { return startDate; } set { startDate = value; } }
        internal DateTime ValuationDate { set { valuationDate = value; } }
        internal double Notional { set { notional = value; } }
        internal int PayFrequency { set { payFrequency = value; } }
        internal DateTime MaturityDate { set { maturityDate = value; } }
        internal List<double> FloatRates { get { return floatRates; } }
        internal double FloatValue { get { return value; } set { value = this.value; } }

        internal FloatingLeg()
        {
            this.floatRates = new List<double>();
        }

        internal double CalcDuration()
        {
            double duration = 0;
            duration = (double)((SwapConstants.PayDates[0] - valuationDate).TotalDays + 1) / SwapConstants.Three_Sixty_Five;
            return duration;
        }

        internal double CalcDV01()
        {
            double duration = CalcDuration();
            double val = this.value;
            double DV = 0;
            DV = -(duration * notional) / (1 / 10000);
            Debug.WriteLine(String.Format("Float DV01 = {0}", DV));
            return -DV;
        }

        internal double CalcModifiedDuration()
        {
            double val = CalcDV01();
            double marketValue = this.value;
            double MD = (val / (notional + marketValue)) * 10000;
            Debug.WriteLine(String.Format("Float modified duration = {0}", MD));
            return MD;
        }

        internal void CalcPayFloat()
        {
            try
            {
                double val = 0;
                double diff = 0;
                int d = 0;
                int d1 = 0;
                double floatVal = 0;

                for (int i = 0; i < (SwapConstants.PayDates.Count-1); i++)
                {
#if DEBUG
                    Debug.WriteLine(String.Format("FloatingLeg - CalPayFloat - i : {0} \n\r", i));
#endif

                    d = (int)(SwapConstants.PayDates[i + 1] - SwapConstants.PayDates[0]).TotalDays + 1;
                    d1 = (int)(SwapConstants.PayDates[i + 1] - SwapConstants.PayDates[i]).TotalDays + 1;

                    diff = (SwapConstants.PayDates[i + 1] - SwapConstants.PayDates[0]).TotalDays + 1;
                    diff = diff / SwapConstants.Three_Sixty_Five;
                    floatVal = SwapConstants.Interpolate(floatLegRate[Math.Floor(diff)], floatLegRate[Math.Ceiling(diff)], Math.Floor(diff), Math.Ceiling(diff), diff);

                    if (payFrequency == 1)
                    {
                        val = notional * ((double)SwapConstants.Thirty / SwapConstants.Three_Sixty) * floatVal;
                    }
                    else
                    {
                        val = notional * ((double)d1 / SwapConstants.Three_Sixty_Five) * floatVal;
                    }
                    floatRates.Add(val);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw ex;
            }
        }
    }

    internal class FixedLeg
    {
        private List<double> payFixedLeg;
        private double fixedLegRate;
        private double fixedLegBasis;
        private double duration;
        private double value;
        private double accrual;
        private double notional;
        private double basis;
        private int payFrequency;
        private DateTime effectiveDate;
        private DateTime maturityDate;
        private DateTime valuationDate;

        internal double Value { get { return value; } set { this.value = value; } }
        internal DateTime ValuationDate { get { return valuationDate; } set { valuationDate = value; } }
        internal DateTime EffectiveDate { get { return effectiveDate; } set { effectiveDate = value; } }
        internal double Notional { get { return notional; } set { notional = value; } }
        internal int PayFrequency { get { return payFrequency; } set { payFrequency = value; } }
        internal double FixedLegRate { get { return fixedLegRate; } set { fixedLegRate = value; } }
        internal DateTime MaturityDate { get { return maturityDate; } set { maturityDate = value; } }
        internal List<double> PayFixedLeg { get { return payFixedLeg; } set { payFixedLeg = value; } }

        internal FixedLeg(List<double> payFixedLeg, double fixedLegRate, double fixedLegBasis, int payFrequency)
        {
            this.payFixedLeg = payFixedLeg;
            this.fixedLegRate = fixedLegRate;
            this.fixedLegBasis = fixedLegBasis;
            this.payFrequency = payFrequency;
        }

        internal FixedLeg()
        {
            this.payFixedLeg = new List<double>();
        }
        
        internal double CalcDuration()
        {
            double sum = 0;
            double val = this.value;
            double duration = 0;
            double dis = 0;

            for (int i = 0; i < (payFixedLeg.Count-1); i++)
            {
                sum += payFixedLeg[i] * ((double)((SwapConstants.PayDates[i + 1] - valuationDate).TotalDays + 1) / ((maturityDate - valuationDate).TotalDays + 1));
            }

            sum += notional;
            duration = sum / val;
            Debug.WriteLine(String.Format("Fixed duration = {0}", duration));

            return duration;
        }

        internal double CalcDV01()
        {
            double duration = CalcDuration();
            double val = this.value;
            double DV = 0;

            DV = (duration * notional) * (1 / 10000);
            Debug.WriteLine(String.Format("Fixed DV01 = {0}", DV));

            return DV;
        }

        internal double CalModifiedDuration()
        {
            double val = CalcDV01();
            double marketValue = this.Value;
            double MD = val / (notional + marketValue) * (1 / 10000);
            Debug.WriteLine(String.Format("Fixed modified duration = {0}", MD));
            return MD;
        }

        internal void CalcPayFixed()
        {

            try
            {
                double val = 0;
                int diff = 0;

                for (int i = 0; i < SwapConstants.PayDates.Count; i++)
                {
                    if (payFrequency == 1)
                    {
                        if ((i + payFrequency) < SwapConstants.PayDates.Count)
                            diff = (int)(SwapConstants.PayDates[i + payFrequency] - SwapConstants.PayDates[i]).TotalDays + 1;
                        else
                            diff = 0;

                        if (i != 0 && (i % payFrequency == 0))
                            val = notional * ((double)SwapConstants.Thirty / SwapConstants.Three_Sixty) * fixedLegRate;
                        else
                            val = 0;
                    }
                    else
                    {
                        if (i + payFrequency < SwapConstants.PayDates.Count)
                        {
                            //subtract five because there are 5 less days in a 360 day year
                            diff = ((int)(SwapConstants.PayDates[i + payFrequency] - SwapConstants.PayDates[i]).TotalDays + 1) - 5;
                        }
                        else
                            diff = 0;

                        if (i > 0 && ((i - 1) % payFrequency == 0))
                        {
                            val = notional * ((double)diff / SwapConstants.Three_Sixty) * fixedLegRate;
                        }
                        else
                            val = 0;
                    }
                    Debug.WriteLine(String.Format("FixedLeg - CalcPayFixed - Val is {0} \n\r", val));
                    Debug.WriteLine(String.Format("FixedLeg - CalcPayFixed - current iteration is {0} \n\r", i));
                    payFixedLeg.Add(val);

                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    public class Swap
    {
        private SwapType swapType;
        private double notional;
        private double fixedAccrued;
        private double floatAccrued;
        private double fixedRate;
        private FloatingLeg floatingLeg;
        private FixedLeg fixedLeg;
        private DateTime maturity;
        private DateTime fixedAccruedDate;
        private DateTime floatAccruedDate;
        private DateTime effectiveDate;
        private DateTime settlementDate;
        DateTime valuationDate;
        private string index;
        private Dictionary<double, double> discRates;
        private Dictionary<double, double> floatRates;

        internal double Notional { get { return notional; } }
        internal Dictionary<double, double> DiscRates { set { discRates = value; } }

        public Swap(double notional, DateTime maturity, DateTime effectiveDate, DateTime settlementDate, DateTime valuationDate, Dictionary<double,double> liborRate, Dictionary<double, double> disc,
            double fixedRate, SwapType swapType)
        {
            CalcPayDates(effectiveDate, maturity, valuationDate);
            this.notional = notional;
            this.maturity = maturity;
            this.effectiveDate = effectiveDate;
            this.settlementDate = settlementDate;
            this.valuationDate = valuationDate;
            this.DiscRates = disc;
            this.floatRates = liborRate;

            this.fixedLeg = new FixedLeg();
            this.floatingLeg = new FloatingLeg();

            //fixed leg
            this.fixedLeg.Notional = notional;
            this.fixedLeg.ValuationDate = valuationDate;
            this.fixedLeg.PayFrequency = 2;
            this.fixedLeg.FixedLegRate = fixedRate;
            this.fixedLeg.MaturityDate = maturity;
            this.fixedLeg.CalcPayFixed();
            this.fixedLeg.EffectiveDate = effectiveDate;

            //floating leg
            this.floatingLeg.EffectiveDate = effectiveDate;
            this.floatingLeg.PayFrequency = 4;
            this.floatingLeg.Notional = notional;
            this.floatingLeg.ValuationDate = valuationDate;
            this.floatingLeg.FloatLegRate = liborRate;

            this.floatingLeg.CalcPayFloat();
            NetPayments();
            CalcDV01();
        }

        internal double CalcDV01()
        {
            double val = 0;
            if (swapType == SwapType.PayFloatReceiveFixed)
                val = fixedLeg.CalcDV01() - floatingLeg.CalcDV01();
            else
                val = floatingLeg.CalcDV01() - fixedLeg.CalcDV01();

            Debug.WriteLine(String.Format("Swap DV01 = {0}", val));
            return val;
        }

        internal void CalcPayDates(DateTime tradeDate, DateTime endDate, DateTime valuation)
        {
            effectiveDate = tradeDate.AddDays(-1);

            if (effectiveDate.DayOfWeek == DayOfWeek.Saturday)
                effectiveDate.AddDays(2);
            else if (effectiveDate.DayOfWeek == DayOfWeek.Sunday)
                effectiveDate.AddDays(1);

            DateTime currDate = effectiveDate;

            int count = 0;
            while (currDate <= endDate)
            {
                currDate = currDate.AddMonths(3);
                while (currDate.Day > effectiveDate.Day)
                {
                    currDate = currDate.AddDays(-1);
                }

                if (currDate <= valuation)
                {
                    if (currDate.DayOfWeek == DayOfWeek.Saturday)
                        currDate = currDate.AddDays(2);
                    else if (currDate.DayOfWeek == DayOfWeek.Sunday)
                        currDate = currDate.AddDays(1);
                    else if (currDate == new DateTime(currDate.Year, 12, 25))
                        currDate = currDate.AddDays(1);

                    fixedAccruedDate = currDate;
                }
                if ((currDate <= endDate) && (currDate >= valuation))
                {
                    if (currDate.DayOfWeek == DayOfWeek.Saturday)
                        currDate = currDate.AddDays(2);
                    else if (currDate.DayOfWeek == DayOfWeek.Sunday)
                        currDate = currDate.AddDays(1);
                    else if (currDate == new DateTime(currDate.Year, 12, 25))
                        currDate = currDate.AddDays(1);

                    SwapConstants.PayDates.Add(currDate);
                    count++;
                }
            }
        }

        internal void NetPayments()
        {
            try
            {
                double val = 0;
                double y = 0;
                List<double> fixedLegPays = this.fixedLeg.PayFixedLeg;
                List<double> floatLegPays = this.floatingLeg.FloatRates;
                double x = 0;
                double sum = 0;
                double sumFixed = 0;
                double sumFloat = 0;

                if (swapType == SwapType.PayFloatReceiveFixed)
                {
                    fixedAccrued = notional * fixedRate * (int)(valuationDate - fixedAccruedDate).TotalDays / SwapConstants.Three_Sixty;
                    floatAccrued = -notional * -floatRates[0] * ((int)(valuationDate - fixedAccruedDate).TotalDays / SwapConstants.Three_Sixty_Five);
                }
                else
                {
                    fixedAccrued = -notional * fixedRate * (valuationDate - fixedAccruedDate).TotalDays / SwapConstants.Three_Sixty;
                    floatAccrued = notional * floatRates[0] * ((int)(valuationDate - fixedAccruedDate).TotalDays / SwapConstants.Three_Sixty_Five);
                }

                for (int i = 0; i<(SwapConstants.PayDates.Count-1); i++)
                {
                    x = ((SwapConstants.PayDates[i+1] - SwapConstants.PayDates[0]).TotalDays + 1)/SwapConstants.Three_Sixty;
                    y = SwapConstants.Interpolate(discRates[Math.Floor(x)], discRates[Math.Ceiling(x)], Math.Floor(x), Math.Ceiling(x), x);

                    if (swapType == SwapType.PayFloatReceiveFixed)
                    {
                        val = (fixedLegPays[i] - floatLegPays[i]) * y;
                        sumFixed += fixedLegPays[i]*y;
                        sumFloat -= floatLegPays[i]*y;
                    }
                    else
                    {
                        val = (floatLegPays[i] - fixedLegPays[i])*y;
                        sumFixed -= fixedLegPays[i];
                        sumFloat += floatLegPays[i];
                    }
                    sum += val;
                }

                fixedLeg.Value = sumFixed;
                floatingLeg.FloatValue = sumFloat;

                Debug.Write(String.Format("Fixed Accrued = {0} \n\r Float Accrued = {1} \n\r Accrued = {2} \n\r Principal = {3} Market value = {4} \n\r",
                    fixedAccrued, floatAccrued, (fixedAccrued + floatAccrued), sum, sum + (fixedAccrued + floatAccrued)));
            }
            catch (Exception ex)
            {
            }

        }
    }
}
