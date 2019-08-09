using System;
using System.Diagnostics;
using System.Text;

namespace elFinder.NetCore.Helpers
{
    public static class HttpEncoder
    {
        public static string DecodePath(string path)
        {
            return Encoding.UTF8.GetString(UrlTokenDecode(path));
        }

        public static string EncodePath(string path)
        {
            return UrlTokenEncode(Encoding.UTF8.GetBytes(path));
        }

        private static byte[] UrlTokenDecode(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            int len = input.Length;
            if (len < 1)
            {
                return new byte[0];
            }

            ///////////////////////////////////////////////////////////////////
            // Step 1: Calculate the number of padding chars to append to this string.
            //         The number of padding chars to append is stored in the last char of the string.
            int numPadChars = input[len - 1] - '0';
            if (numPadChars < 0 || numPadChars > 10)
            {
                return null;
            }

            ///////////////////////////////////////////////////////////////////
            // Step 2: Create array to store the chars (not including the last char)
            //          and the padding chars
            char[] base64Chars = new char[len - 1 + numPadChars];

            ////////////////////////////////////////////////////////
            // Step 3: Copy in the chars. Transform the "-" to "+", and "*" to "/"
            for (int iter = 0; iter < len - 1; iter++)
            {
                char c = input[iter];

                switch (c)
                {
                    case '-': base64Chars[iter] = '+'; break;
                    case '_': base64Chars[iter] = '/'; break;
                    default: base64Chars[iter] = c; break;
                }
            }

            ////////////////////////////////////////////////////////
            // Step 4: Add padding chars
            for (int iter = len - 1; iter < base64Chars.Length; iter++)
            {
                base64Chars[iter] = '=';
            }

            // Do the actual conversion
            return Convert.FromBase64CharArray(base64Chars, 0, base64Chars.Length);
        }

        private static string UrlTokenEncode(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (input.Length < 1)
            {
                return string.Empty;
            }

            string base64Str = null;
            int endPos = 0;
            char[] base64Chars = null;

            ////////////////////////////////////////////////////////
            // Step 1: Do a Base64 encoding
            base64Str = Convert.ToBase64String(input);
            if (base64Str == null)
            {
                return null;
            }

            ////////////////////////////////////////////////////////
            // Step 2: Find how many padding chars are present in the end
            for (endPos = base64Str.Length; endPos > 0; endPos--)
            {
                if (base64Str[endPos - 1] != '=') // Found a non-padding char!
                {
                    break; // Stop here
                }
            }

            ////////////////////////////////////////////////////////
            // Step 3: Create char array to store all non-padding chars,
            //      plus a char to indicate how many padding chars are needed
            base64Chars = new char[endPos + 1];
            base64Chars[endPos] = (char)('0' + base64Str.Length - endPos); // Store a char at the end, to indicate how many padding chars are needed

            ////////////////////////////////////////////////////////
            // Step 3: Copy in the other chars. Transform the "+" to "-", and "/" to "_"
            for (int iter = 0; iter < endPos; iter++)
            {
                char c = base64Str[iter];

                switch (c)
                {
                    case '+': base64Chars[iter] = '-'; break;
                    case '/': base64Chars[iter] = '_'; break;
                    case '=': Debug.Assert(false); base64Chars[iter] = c; break;
                    default: base64Chars[iter] = c; break;
                }
            }
            return new string(base64Chars);
        }
    }
}