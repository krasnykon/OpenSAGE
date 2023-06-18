using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace OpenSage.Content.Translation
{
    public static class TranslationProvider
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(string str)
        {
            return TranslationManager.Instance.GetString(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(string str, EventHandler languageChangedCallback)
        {
            var result = TranslationManager.Instance.GetString(str);
            TranslationManager.Instance.LanguageChanged += languageChangedCallback;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetStringFormatted(string str, params object[] args)
        {
            return string.Format(TranslationManager.Instance.GetString(str), args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetStringFormatted(string str, EventHandler languageChangedCallback, params object[] args)
        {
            var result = string.Format(TranslationManager.Instance.GetString(str), args);
            TranslationManager.Instance.LanguageChanged += languageChangedCallback;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetParticularString(string context, string str)
        {
            return TranslationManager.Instance.GetParticularString(context, str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetParticularString(string context, string str, EventHandler languageChangedCallback)
        {
            var result = TranslationManager.Instance.GetParticularString(context, str);
            TranslationManager.Instance.LanguageChanged += languageChangedCallback;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetParticularStringFormatted(string context, string str, params object[] args)
        {
            return string.Format(TranslationManager.Instance.GetParticularString(context, str), args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetParticularStringFormatted(string context, string str, EventHandler languageChangedCallback, params object[] args)
        {
            var result = string.Format(TranslationManager.Instance.GetParticularString(context, str), args);
            TranslationManager.Instance.LanguageChanged += languageChangedCallback;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnregisterLanguageChangedCallback(EventHandler languageChangedCallback)
        {
            TranslationManager.Instance.LanguageChanged -= languageChangedCallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Translate(this string str)
        {
            return GetString(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Translate(this string str, string context)
        {
            return GetParticularString(context, str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TranslateFormatted(this string str, params object[] args)
        {
            string fmtStr = FormatConvert(GetString(str));
            return string.Format(fmtStr, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TranslateFormatted(this string str, string context, params object[] args)
        {
            string fmtStr = FormatConvert(GetParticularString(context, str));
            return string.Format(fmtStr, args);
        }

        private static string FormatConvert(string format)
        {
            Regex pattern = new Regex(@"(%[xXeEfFdDgG])");
            uint cnt = 0;
            MatchEvaluator evaluator = (match) =>
            {
                string ret = String.Format("{{{0}}}", cnt);
                cnt++;
                return ret;
            };
            return pattern.Replace(format, evaluator);
        }
        
    }
}
