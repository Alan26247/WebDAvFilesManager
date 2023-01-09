using System.Text;

namespace WebDavFilesRepository.Server.Additions
{
    public static class GeneratorRandom
    {
        public static string GenerateName(int countChars)
        {
            StringBuilder returnName = new();
            Random random = new();

            for (int i = 0; i < countChars; i++)
            {
                char index = (char)random.Next(97, 122);

                returnName.Append(index);
            }

            return returnName.ToString();
        }
    }
}
