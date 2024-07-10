using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class TextReplacer
{
    // 문장에 있는 특정 Keyword를 Replace하는 함수.
    // textsByKeyword의 Key는 Replace 대상 keyword, Value는 Replace해 넣을 Value
    public static string Replace(string text, IReadOnlyDictionary<string, string> textsByKeyword)
    {
        if (textsByKeyword != null)
        {
            // Text = "오늘 날씨는 $[Test]도입니다.", Key = "Test", Value = "10";
            // => "오늘 날씨는 10도입니다."
            foreach (var pair in textsByKeyword)
                text = text.Replace($"$[{pair.Key}]", pair.Value);
        }
        return text;
    }

    public static string Replace(string text, string prefixKeyword, IReadOnlyDictionary<string, string> textsByKeyword, bool useStack)
    {
        if (textsByKeyword != null)
        {
            foreach (var pair in textsByKeyword)
                text = text.Replace($"[{prefixKeyword}.{pair.Key}]", pair.Value);
        }
        return text;
    }

    public static string Replace(string text, string prefixKeyword, IReadOnlyDictionary<string, string> textsByKeyword)
    {
        if (textsByKeyword != null)
        {
            foreach (var pair in textsByKeyword)
                text = text.Replace($"$[{prefixKeyword}.{pair.Key}]", pair.Value);
        }
        return text;
    }

    public static string Replace(string text, IReadOnlyDictionary<string, string> textsByKeyword, string suffixKeyword)
    {
        if (textsByKeyword != null)
        {
            foreach (var pair in textsByKeyword)
                text = text.Replace($"$[{pair.Key}.{suffixKeyword}]", pair.Value);
        }
        return text;
    }

    public static string Replace(string text, string prefixKeyword, IReadOnlyDictionary<string, string> textsByKeyword, string suffixKeyword)
    {
        if (textsByKeyword != null)
        {
            foreach (var pair in textsByKeyword)
                text = text.Replace($"$[{prefixKeyword}.{pair.Key}.{suffixKeyword}]", pair.Value);
        }
        return text;
    }

    public static string Replace(string text, string prefixKeyword, IReadOnlyDictionary<string, string> textsByKeyword, string suffixKeyword, bool useStack)
    {
        if (textsByKeyword != null)
        {
            foreach (var pair in textsByKeyword)
                text = text.Replace($"[{prefixKeyword}.{pair.Key}.{suffixKeyword}]", pair.Value);
        }
        return text;
    }
}
