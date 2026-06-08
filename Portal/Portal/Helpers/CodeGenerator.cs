using System;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.Helpers
{
    /// <summary>
    /// Gera códigos alfanuméricos únicos de 6 caracteres (ex.: K9X2WP).
    /// Utilizado para códigos de adesão de turmas e de acesso a desafios.
    /// </summary>
    public static class CodeGenerator
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int CodeLength = 6;

        public static async Task<string> GenerateUniqueAsync(Func<string, Task<bool>> isTaken)
        {
            string code;
            do
            {
                code = new string(Enumerable.Range(0, CodeLength)
                    .Select(_ => Chars[Random.Shared.Next(Chars.Length)])
                    .ToArray());
            } while (await isTaken(code));
            return code;
        }
    }
}
