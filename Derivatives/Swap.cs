using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Derivatives
{
    public static class SwapConstants
    {
        public const int NumDates = 100;
        public const int Thirty = 30;
        public const int Three_Sixty = 360;
        public const int Three_Sixty_Five = 365;
        public const int Notional = 100000;

        public static List<DateTime> PayDates;

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

        double CalcDuration()
        {
            double duration = 0;
            duration = (double)((SwapConstants.PayDates[0] - valuationDate).TotalDays + 1) / SwapConstants.Three_Sixty_Five;
            return duration;
        }

        double CalcDV01()
        {
            double duration = CalcDuration();
            double val = this.value;
            double DV = 0;
            DV = -(duration * notional) / (1 / 10000);
            Debug.WriteLine(String.Format("Float DV01 = {0}", DV));
            return -DV;
        }

        double CalcModifiedDuration()
        {
            double val = CalcDV01();
            double marketValue = this.value;
            double MD = (val / (notional + marketValue)) * 10000;
            Debug.WriteLine(String.Format("Float modified duration = {0}", MD));
            return MD;
        }

        void CalcPayFloat()
        {
            double val = 0;
            double diff = 0;
            int d = 0;
            int d1 = 0;
            double floatVal = 0;

            for (int i = 0; i < SwapConstants.PayDates.Count; i++)
            {
                d = (int)(SwapConstants.PayDates[i + 1] - SwapConstants.PayDates[0]).TotalDays + 1;
                d1 = (int)(SwapConstants.PayDates[i + 1] - SwapConstants.PayDates[i]).TotalDays + 1;

                diff = (SwapConstants.PayDates[i + 1] - SwapConstants.PayDates[0]).TotalDays + 1;
                diff = diff / SwapConstants.Three_Sixty_Five;
                floatVal = SwapConstants.Interpolate(floatLegRate[Math.Floor(diff)], floatLegRate[Math.Ceiling(diff)], Math.Floor(diff), Math.Ceiling(diff), diff);

                if (payFrequency == 1)
                {
                    val = notional * (SwapConstants.Thirty / SwapConstants.Three_Sixty) * floatVal;
                }
                else
                {
                    val = notional * (d1 / SwapConstants.Three_Sixty_Five) * floatVal;
                }
                floatRates.Add(val);
            }
        }
    }

    class FixedLeg
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

        double Value { get { return value; } set { this.value = value; } }
        DateTime ValuationDate { set { valuationDate = value; } }
        double Notional { set { notional = value; } }
        int PayFrequency { set { payFrequency = value; } }
        double FixedLegRate { set { fixedLegRate = value; } }
        DateTime MaturityDate { set { maturityDate = value; } }
        List<double> PayFixedLeg { set { payFixedLeg = value; } }

        FixedLeg(List<double> payFixedLeg, double fixedLegRate, double fixedLegBasis, int payFrequency)
        {
            this.payFixedLeg = payFixedLeg;
            this.fixedLegRate = fixedLegRate;
            this.fixedLegBasis = fixedLegBasis;
            this.payFrequency = payFrequency;
        }

        double CalcDuration()
        {
            double sum = 0;
            double val = this.value;
            double duration = 0;
            double dis = 0;

            for (int i = 0; i < payFixedLeg.Count; i++)
            {
                sum += payFixedLeg[i] * ((double)((SwapConstants.PayDates[i + 1] - valuationDate).TotalDays + 1) / ((maturityDate - valuationDate).TotalDays + 1));
            }

            sum += notional;
            duration = sum / val;
            Debug.WriteLine(String.Format("Fixed duration = {0}", duration));

            return duration;
        }

        double CalcDV01()
        {
            double duration = CalcDuration();
            double val = this.value;
            double DV = 0;

            DV = (duration * notional) * (1 / 10000);
            Debug.WriteLine(String.Format("Fixed DV01 = {0}", DV));

            return DV;
        }

        double CalModifiedDuration()
        {
            double val = CalcDV01();
            double marketValue = this.Value;
            double MD = val / (notional + marketValue) * (1 / 10000);
            Debug.WriteLine(String.Format("Fixed modified duration = {0}", MD));
            return MD;
        }

        void CalcPayFixed()
        {
            double val = 0;
            int diff = 0;
            DateTime dateDiff;

            for (int i = 0; i < SwapConstants.PayDates.Count; i++)
            {
                if (payFrequency == 1)
                {
                    if ((i + payFrequency) < SwapConstants.PayDates.Count)
                        diff = (int)(SwapConstants.PayDates[i + payFrequency] - SwapConstants.PayDates[i]).TotalDays + 1;
                    else
                        diff = 0;

                    if (i != 0 && (i % payFrequency == 0))
                        val = notional * (SwapConstants.Thirty / SwapConstants.Three_Sixty) * fixedLegRate;
                    else
                        val = 0;
                }
                else
                {
                    if (i + payFrequency <= SwapConstants.PayDates.Count)
                    {
                        //subtract five because there are 5 less days in a 360 day year
                        diff = ((int)(SwapConstants.PayDates[i + payFrequency] - SwapConstants.PayDates[i]).TotalDays + 1) - 5;
                    }
                    else
                        diff = 0;

                    if (i > 0 && ((i - 1) % payFrequency == 0))
                    {
                        val = notional * (diff / SwapConstants.Three_Sixty) * fixedLegRate;
                    }
                    else
                        val = 0;
                }
            }

            payFixedLeg.Add(val);
        }
    }

    public class Swap
    {
    }
}
