namespace UtilityLibrary
{
    public static class MyRandomGenerator
    {
        public static string GenerateAlphanumericValue(int numCaratteri)
        {
            string caratteri = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string valore = "";
            for (int i = 0; i < numCaratteri; i++)
            {
                int indice = random.Next(caratteri.Length);
                valore += caratteri[indice];
            }
            return valore;
        }
    }
}