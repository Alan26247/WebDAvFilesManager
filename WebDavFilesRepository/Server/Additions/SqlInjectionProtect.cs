namespace WebDavFilesRepository.Server.Additions
{
    public static class SqlInjectionProtect
    {
        public static bool Check(string value)
        {
            bool valid = true;

            // проверяем на все запрещенные символы и комбинации
            // согласно рекомендациям microsoft

            // запрещены - ; ' ` " -- /* */ xp_
            if (value.Contains(';')) valid = false;
            if (value.Contains('\'')) valid = false;
            else if (value.Contains('`')) valid = false;
            else if (value.Contains('\"')) valid = false;
            else if (value.Contains("--")) valid = false;
            else if (value.Contains("/*")) valid = false;
            else if (value.Contains("*/")) valid = false;
            else if (value.Contains("xp_")) valid = false;

            // также запрещены комбинации
            else if (value.Contains("AUX")) valid = false;
            else if (value.Contains("CLOCK$")) valid = false;
            else if (value.Contains("COM1")) valid = false;
            else if (value.Contains("COM2")) valid = false;
            else if (value.Contains("COM3")) valid = false;
            else if (value.Contains("COM4")) valid = false;
            else if (value.Contains("COM5")) valid = false;
            else if (value.Contains("COM6")) valid = false;
            else if (value.Contains("COM7")) valid = false;
            else if (value.Contains("COM8")) valid = false;
            else if (value.Contains("CON")) valid = false;
            else if (value.Contains("CONFIG$")) valid = false;
            else if (value.Contains("LPT1")) valid = false;
            else if (value.Contains("LPT2")) valid = false;
            else if (value.Contains("LPT3")) valid = false;
            else if (value.Contains("LPT4")) valid = false;
            else if (value.Contains("LPT5")) valid = false;
            else if (value.Contains("LPT6")) valid = false;
            else if (value.Contains("LPT7")) valid = false;
            else if (value.Contains("LPT8")) valid = false;
            else if (value.Contains("NUL")) valid = false;
            else if (value.Contains("PRN")) valid = false;

            return valid;
        }
    }
}
