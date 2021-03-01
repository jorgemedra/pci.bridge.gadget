using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OD_Finesse_Bridge.utils
{
    class Amounts
    {
        public static bool isNumber(string amount)
        {
            bool isNum = false;
            try
            {
                decimal dec = Convert.ToDecimal(amount);
                isNum = true;
            }catch(ArithmeticException ae)
            {
            }
            return isNum;
        }
        public static bool isValid(string amount)
        {
            bool valid = false;
            Regex Val = new Regex(@"^(\d{1}\.)?(\d+\.?)+(,\d{2})?$");
            if(Val.IsMatch(amount))
            {
                valid = true;
            }
            return valid;
        }

        public static bool isDecimal(string amount)
        {
            bool isDec = false;
            if(amount.IndexOf(',')>-1 || amount.IndexOf('.')>-1)
            {
                isDec = true;
            }
            return isDec;
        }

        public static string amountFormatFix(string amount)
        {
            string newAmount = "";
            int numOfDec = getNumOfDec(amount);
            if(numOfDec<2)
            {
                newAmount = amount + "0";
            }
            else
            {
                throw new InvalidDecimalException("Se han encontrado más de 2 decimales. Formato de monto incorrecto.");
            }
            return newAmount;
        }

        private static int getNumOfDec(string amt)
        {
            int decs = 0;
            int indxStart = amt.IndexOf(",");
            string aux = amt.Substring(indxStart + 1);
            decs = aux.Length;
            Console.WriteLine("Decimales: " + decs);
            return decs;
        }

        private static string getInt(string amt)
        {
            string newAmt = "";
            string aux = amt.Substring(0, amt.IndexOf(","));
            newAmt = aux;
            return newAmt;
        }

        private static string getTwDec(string amt)
        {
            string newAmt = "";
            int indxStart = amt.IndexOf(",");
            string aux = amt.Substring(indxStart + 1);
            string twoDec = aux.Substring(0, 2);
            int trdDec = Convert.ToInt32(aux.Substring(2,1));
            if(trdDec>=5)
            {
                newAmt = "" + (Convert.ToInt32(twoDec) + 1);
            }
            else
            {
                newAmt = twoDec;
            }
            return newAmt;
        }

        public static string validaMonto(string amnt)
        {
            string mnt = amnt.Replace(".", ",");
            if (isNumber(mnt))
            {
                if (isDecimal(mnt))
                {
                    if (!isValid(mnt))
                    {
                        mnt = amountFormatFix(mnt);
                    }
                }
                else
                {
                    mnt += ".00";
                }
                mnt = mnt.Replace(",", ".");
                return mnt;
            }
            else
            {
                return mnt;
            }
        }
    }

}
