using System;
using UnityEngine;

public static class TextEllipsis
{
    /// <summary>
    /// 依字數限制
    /// 設定字符串過長顯示省略符號
    /// </summary>
    /// <param name="targetStr"></param>
    /// <param name="maxWidth"></param>
    /// <param name="suffix"></param>
    /// <returns></returns>
    public static string SetTextWithEllipsis(string targetStr, int maxWidth = 14, string suffix = "…")
    {
        if (string.IsNullOrEmpty(targetStr) || maxWidth <= 0)
            return targetStr;

        if (targetStr.Length * 2 <= maxWidth)
            return targetStr;

        Char[] chars = targetStr.ToCharArray();
        int curLen = 0;
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] >= 0x4e00 && chars[i] <= 0x9fff)
                curLen += 2;
            else
                curLen += 1;

            if (curLen > maxWidth)
                return targetStr.Substring(0, i) + suffix;
        }
        return targetStr;
    }

    /// <summary>
    /// 依長度限制
    /// 設定字符串過長顯示省略符號
    /// 排行版maxWidth = 1125
    /// </summary>   
    public static void SetTextWithEllipsis(this UnityEngine.UI.Text text, string value, int maxWidth)
    {
        text.text = StripLengthWithSuffix(text, value, maxWidth);
    }

    public static void SetTextWithEllipsis(this UnityEngine.UI.Text text, string value)
    {
        text.text = StripLengthWithSuffix(text, value, Mathf.FloorToInt(text.rectTransform.rect.width));
    }

    private const string suffix = "…";
    static string StripLengthWithSuffix(UnityEngine.UI.Text text, string input, int maxWidth)
    {
        int len = CalculateLengthOfText(text, input);

        int suffixWidth = CalculateLengthOfText(text, suffix);
        //截斷text的長度，如果總長度大於限制的最大長度，
        //那麼先根據最大長度減去後綴長度的值拿到字符串，在拼接上後綴
        if (len > maxWidth)
            return StripLength(text, input, maxWidth - suffixWidth) + suffix;
        else
            return input;
    }

    /// <summary>
    /// 根據maxWidth來截斷input拿到子字符串
    /// </summary>        
    static string StripLength(UnityEngine.UI.Text text, string input, int maxWidth)
    {
        int totalLength = 0;
        UnityEngine.Font myFont = text.font;  //chatText is my Text component
        myFont.RequestCharactersInTexture(input, text.fontSize, text.fontStyle);
        UnityEngine.CharacterInfo characterInfo;
        char[] arr = input.ToCharArray();
        int i = 0;
        foreach (char c in arr)
        {
            myFont.GetCharacterInfo(c, out characterInfo, text.fontSize, text.fontStyle);
            int newLength = totalLength + characterInfo.advance;
            if (newLength > maxWidth)
            {
                //if (UnityEngine.Mathf.Abs(newLength - maxWidth) > UnityEngine.Mathf.Abs(maxWidth - totalLength))
                //{
                break;
                //}
                //else
                //{
                //    totalLength = newLength;
                //    i++;
                //    break;
                //}
            }
            totalLength += characterInfo.advance;
            i++;
        }

        return input.Substring(0, i);
    }

    /// <summary>
    /// 取的文字長度
    /// </summary>
    /// <param name="text"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static int CalculateLengthOfText(UnityEngine.UI.Text text, string message)
    {
        int totalLength = 0;
        UnityEngine.Font myFont = text.font;  //chatText is my Text component
        myFont.RequestCharactersInTexture(message, text.fontSize, text.fontStyle);
        UnityEngine.CharacterInfo characterInfo;
        char[] arr = message.ToCharArray();
        foreach (char c in arr)
        {
            myFont.GetCharacterInfo(c, out characterInfo, text.fontSize, text.fontStyle);
            totalLength += characterInfo.advance;
        }
        return totalLength;
    }
}
