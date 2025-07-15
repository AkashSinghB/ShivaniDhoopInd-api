namespace api_InvoicePortal.Services
{
    public static class NumToWordsConverter
    {
        private static string ConvertNumberToWordsIndianFormat(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + ConvertNumberToWordsIndianFormat(Math.Abs(number));

            string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
                       "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen",
                       "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            string words = "";

            long crore = number / 10000000;
            number %= 10000000;
            if (crore > 0)
                words += ConvertNumberToWordsIndianFormat(crore) + " Crore ";

            long lakh = number / 100000;
            number %= 100000;
            if (lakh > 0)
                words += ConvertNumberToWordsIndianFormat(lakh) + " Lakh ";

            long thousand = number / 1000;
            number %= 1000;
            if (thousand > 0)
                words += ConvertNumberToWordsIndianFormat(thousand) + " Thousand ";

            long hundred = number / 100;
            number %= 100;
            if (hundred > 0)
                words += ConvertNumberToWordsIndianFormat(hundred) + " Hundred ";

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                if (number < 20)
                    words += units[number];
                else
                {
                    words += tens[number / 10];
                    if ((number % 10) > 0)
                        words += " " + units[number % 10];
                }
            }

            return words.Trim();
        }

        public static string AmountToWordsIndianFormat(decimal amount)
        {
            long rupees = (long)amount;
            int paise = (int)((amount - rupees) * 100);

            string result = ConvertNumberToWordsIndianFormat(rupees) + " Rupees";
            if (paise > 0)
                result += " and " + ConvertNumberToWordsIndianFormat(paise) + " Paise";

            return result + " Only";
        }
    }
}
