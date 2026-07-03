using System.Text;

namespace HackerTerminal;

public static class Cipher
{
    public static string Encode(string text, int key) => Shift(text, key);

    public static string Decode(string text, int key) => Shift(text, -key);

    private static string Shift(string text, int key)
    {
        key = ((key % 26) + 26) % 26;
        var sb = new StringBuilder(text.Length);

        foreach (char c in text)
        {
            if (c is >= 'A' and <= 'Z')
                sb.Append((char)('A' + ((c - 'A' + key) % 26 + 26) % 26));
            else if (c is >= 'a' and <= 'z')
                sb.Append((char)('a' + ((c - 'a' + key) % 26 + 26) % 26));
            else
                sb.Append(c);
        }

        return sb.ToString();
    }
}
