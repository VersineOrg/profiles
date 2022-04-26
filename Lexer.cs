using System;
using System.Collections.Generic;

namespace profiles
{
    public static class Lexer
    {
        public static List<Token> Lex(string path)
        {
            List<Token> Lexed = new List<Token>();
            String temp = "";
            foreach (var c in path)
            {
                if (c == '/')
                {
                    Lexed.Add(new Token(temp));
                    temp = "";
                }
                else
                {
                    temp += c;
                }
            }

            return Lexed;
        }
    }
}