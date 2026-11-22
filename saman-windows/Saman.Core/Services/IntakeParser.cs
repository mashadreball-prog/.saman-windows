// Saman Windows | Majid Rastegarmoghaddam | Digi Service
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Saman.Core.Models;
namespace Saman.Core.Services
{
    public class ParseResult
    {
        public Trade? Trade; public string Type, Brand, Model, Size, Name, Phone;
        public long Estimate, Advance;
        public List<string> Symptoms = new List<string>(), Accessories = new List<string>();
    }

    /// Offline rule engine: typed text or speech transcript -> intake fields.
    /// Ported from the approved HTML preview (e.js). Not compiled yet: build in Visual Studio.
    public class IntakeParser
    {
        static readonly Dictionary<string, int> NV = new Dictionary<string, int> {
            {"صفر",0},{"یک",1},{"دو",2},{"سه",3},{"چهار",4},{"پنج",5},{"شش",6},{"هفت",7},{"هشت",8},{"نه",9},{"ده",10},
            {"یازده",11},{"دوازده",12},{"سیزده",13},{"چهارده",14},{"پانزده",15},{"شانزده",16},{"هفده",17},{"هجده",18},{"نوزده",19},
            {"بیست",20},{"سی",30},{"چهل",40},{"پنجاه",50},{"شصت",60},{"هفتاد",70},{"هشتاد",80},{"نود",90},
            {"صد",100},{"یکصد",100},{"دویست",200},{"سیصد",300},{"چهارصد",400},{"پانصد",500},{"ششصد",600},{"هفتصد",700},{"هشتصد",800},{"نهصد",900}};
        static readonly Dictionary<string, long> NM = new Dictionary<string, long> { {"هزار",1000},{"میلیون",1000000},{"میلیارد",1000000000} };

        static readonly Dictionary<Trade, string[][]> Types = new Dictionary<Trade, string[][]> {
            { Trade.Car, new[]{ new[]{"ایسییو","ECU"}, new[]{"ecu","ECU"}, new[]{"کلاستر","کلاستر"}, new[]{"ایموبیلایزر","ایموبیلایزر"} } },
            { Trade.Board, new[]{ new[]{"اینورتر","اینورتر"}, new[]{"مادربرد","مادربرد"}, new[]{"پاور","پاور"}, new[]{"درایو","درایو"}, new[]{"برد","برد الکترونیکی"} } },
            { Trade.Computer, new[]{ new[]{"لپ تاپ","لپ‌تاپ"}, new[]{"لپتاپ","لپ‌تاپ"}, new[]{"کامپیوتر","کامپیوتر"}, new[]{"مانیتور","مانیتور"}, new[]{"پرینتر","پرینتر"} } },
            { Trade.AudioVideo, new[]{ new[]{"تلویزیون","تلویزیون"}, new[]{"تی وی","تلویزیون"}, new[]{"آمپلی فایر","آمپلی‌فایر"}, new[]{"ساندبار","ساندبار"}, new[]{"اسپیکر","اسپیکر"}, new[]{"رسیور","رسیور"} } },
            { Trade.Appliance, new[]{ new[]{"یخچال","یخچال"}, new[]{"لباسشویی","ماشین لباسشویی"}, new[]{"ظرفشویی","ظرفشویی"}, new[]{"جاروبرقی","جاروبرقی"}, new[]{"کولر","کولر"}, new[]{"مایکروفر","مایکروفر"} } },
            { Trade.Mobile, new[]{ new[]{"گوشی","گوشی موبایل"}, new[]{"موبایل","گوشی موبایل"}, new[]{"تبلت","تبلت"} } } };

        // word, display brand, default trade
        static readonly string[][] Brands = {
            new[]{"سامسونگ","Samsung","Mobile"}, new[]{"سونی","Sony","AudioVideo"}, new[]{"ال جی","LG","AudioVideo"}, new[]{"الجی","LG","AudioVideo"},
            new[]{"اسنوا","Snowa","AudioVideo"}, new[]{"ایکس ویژن","X.Vision","AudioVideo"}, new[]{"پاناسونیک","Panasonic","AudioVideo"},
            new[]{"دوو","Daewoo","Appliance"}, new[]{"بوش","Bosch","Appliance"}, new[]{"شیائومی","Xiaomi","Mobile"}, new[]{"هواوی","Huawei","Mobile"},
            new[]{"آنر","Honor","Mobile"}, new[]{"لنوو","Lenovo","Computer"}, new[]{"ایسوس","Asus","Computer"}, new[]{"اچ پی","HP","Computer"},
            new[]{"ایسر","Acer","Computer"}, new[]{"یاماها","Yamaha","AudioVideo"}, new[]{"زیمنس","Siemens","Board"},
            new[]{"پژو","پژو","Car"}, new[]{"سمند","سمند","Car"}, new[]{"پراید","پراید","Car"}, new[]{"تیبا","تیبا","Car"}, new[]{"دنا","دنا","Car"}, new[]{"رنو","Renault","Car"} };

        static readonly Dictionary<Trade, string[]> Symptoms = new Dictionary<Trade, string[]> {
            { Trade.Mobile, new[]{"شکستگی LCD","تاچ نمی‌کند","شارژ نمی‌شود","روشن نمی‌شود","آب‌خوردگی","نرم‌افزار","باتری"} },
            { Trade.AudioVideo, new[]{"تصویر ندارد","صدا ندارد","خط روی صفحه","روشن نمی‌شود","بک‌لایت","پنل شکسته","HDMI"} },
            { Trade.Appliance, new[]{"آب‌گیری نمی‌کند","تخلیه نمی‌کند","سر و صدا","پمپ","برد کنترل","نشتی","گرم نمی‌کند"} },
            { Trade.Car, new[]{"چراغ موتور","استارت نمی‌خورد","ایموبیلایزر","ریموت","کلاستر","ضبط","سنسور"} },
            { Trade.Computer, new[]{"روشن نمی‌شود","صفحه سیاه","هنگ","داغی","باتری","هارد","ویندوز"} },
            { Trade.Board, new[]{"اتصال کوتاه","سوختگی","آی‌سی داغ","ولتاژ ندارد","بایوس","خازن","ریبال BGA"} } };
        static readonly Dictionary<Trade, string[]> Acc = new Dictionary<Trade, string[]> {
            { Trade.Mobile, new[]{"کاور","سیم‌کارت","کارت حافظه","قاب","شارژر"} },
            { Trade.AudioVideo, new[]{"ریموت","کابل برق","پایه","کارتن"} },
            { Trade.Appliance, new[]{"شیلنگ","درب","فیلتر","سیم برق"} },
            { Trade.Car, new[]{"سوئیچ","ریموت","فیش ECU","ضبط"} },
            { Trade.Computer, new[]{"شارژر","کیف","موس","باتری"} },
            { Trade.Board, new[]{"فیش","هیت‌سینک","کابل","دیتاشیت"} } };
        static readonly string[] NameTriggers = { "برای","مشتری","اسمش","آقای","خانم","خانوم","جناب","دکتر","مهندس" };
        static readonly string[] Stop = { "تلویزیون","گوشی","با","که","و","را","رو","آورده","هست","داره","ایراد","مشکل","پیش","برآورد","هزینه","شماره","موبایل","تلفن" };

        static bool IsNum(string t) { return NV.ContainsKey(t) || NM.ContainsKey(t) || Regex.IsMatch(t, @"^\d+$"); }

        public static string Normalize(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s ?? "")
            {
                if (c == 'ي') sb.Append('ی'); else if (c == 'ك') sb.Append('ک');
                else if (c >= '۰' && c <= '۹') sb.Append((char)('0' + (c - '۰')));
                else if (c >= '٠' && c <= '٩') sb.Append((char)('0' + (c - '٠')));
                else if ("\u200c\u200f,،.؟?!".IndexOf(c) >= 0) sb.Append(' ');
                else sb.Append(c);
            }
            return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        }
        static string Squash(string s) { return Normalize(s).Replace(" ", "").Replace("نداره", "ندارد").Replace("نمیکنه", "نمیکند"); }

        /// Converts spoken numbers ("دو میلیون و پانصد هزار") to digits; digit-by-digit runs of 7+ words become phone digits.
        public static string Numerize(string text)
        {
            var tk = Normalize(text).Split(' '); var outp = new List<string>();
            for (int i = 0; i < tk.Length;)
            {
                if (!IsNum(tk[i])) { outp.Add(tk[i++]); continue; }
                int j = i; var run = new List<string>();
                while (j < tk.Length && (IsNum(tk[j]) || (tk[j] == "و" && j + 1 < tk.Length && IsNum(tk[j + 1]) && run.Count > 0)))
                { if (tk[j] != "و") run.Add(tk[j]); j++; }
                if (run.Count == 1 && Regex.IsMatch(run[0], @"^\d{4,}$")) outp.Add(run[0]);
                else if (run.Count >= 7 && run.All(w => NV.ContainsKey(w) && NV[w] <= 9)) outp.Add(string.Concat(run.Select(w => NV[w].ToString())));
                else
                {
                    long tot = 0, cur = 0;
                    foreach (var w in run)
                    {
                        if (NM.ContainsKey(w)) { tot += (cur == 0 ? 1 : cur) * NM[w]; cur = 0; }
                        else cur += Regex.IsMatch(w, @"^\d+$") ? long.Parse(w) : NV[w];
                    }
                    outp.Add((tot + cur).ToString());
                }
                i = j; if (i < tk.Length && (tk[i] == "تومان" || tk[i] == "تومن")) i++;
            }
            return string.Join(" ", outp);
        }

        public ParseResult Parse(string raw)
        {
            var r = new ParseResult(); var T = Numerize(raw); var t = " " + T + " "; var sq = Squash(T);
            foreach (var kv in Types) { foreach (var p in kv.Value) if (t.Contains(" " + p[0])) { r.Trade = kv.Key; r.Type = p[1]; break; } if (r.Trade != null) break; }
            foreach (var b in Brands) if (t.Contains(" " + b[0])) { r.Brand = b[1]; t = t.Replace(b[0], " "); if (r.Trade == null) r.Trade = (Trade)Enum.Parse(typeof(Trade), b[2]); break; }
            var m = Regex.Match(t, @"\b(0?9\d{9})\b"); if (m.Success) { r.Phone = m.Groups[1].Value.Length == 10 ? "0" + m.Groups[1].Value : m.Groups[1].Value; t = t.Replace(m.Groups[1].Value, " "); }
            m = Regex.Match(t, @"(?:پیش ?دریافت|بیعانه|پیش)\s+(\d{3,})"); if (m.Success) { r.Advance = long.Parse(m.Groups[1].Value); t = t.Replace(m.Value, " "); }
            m = Regex.Match(t, @"(?:برآورد|هزینه|قیمت|حدود)\s+(\d{3,})"); if (m.Success) { r.Estimate = long.Parse(m.Groups[1].Value); t = t.Replace(m.Value, " "); }
            m = Regex.Match(t, @"(\d{2,3})\s*(?:اینچ|اینچی)"); if (m.Success) { r.Size = m.Groups[1].Value; t = t.Replace(m.Value, " "); }
            else if (r.Type == "تلویزیون") { m = Regex.Match(t, @"\s(\d{2,3})\s"); int n; if (m.Success && int.TryParse(m.Groups[1].Value, out n) && n >= 19 && n <= 110) { r.Size = m.Groups[1].Value; t = t.Replace(m.Value, " "); } }
            var latin = Regex.Matches(t, @"\b[A-Za-z][A-Za-z0-9\-]*\b|\b\d{1,3}[A-Za-z]+\b").Cast<Match>().Select(x => x.Value)
                .Where(x => !Regex.IsMatch(x, @"^(LCD|ECU|IMEI|SSD|HDMI|BGA|USB|LED)$", RegexOptions.IgnoreCase)).ToList();
            if (latin.Count > 0) r.Model = string.Join(" ", latin);
            var w2 = t.Trim().Split(' ');
            for (int i = 0; i < w2.Length && r.Name == null; i++)
                if (NameTriggers.Contains(w2[i])) { var nm = new List<string>(); int j = i + 1; while (j < w2.Length && nm.Count < 2 && !Regex.IsMatch(w2[j], @"\d") && !Stop.Contains(w2[j])) nm.Add(w2[j++]); if (nm.Count > 0) r.Name = string.Join(" ", nm); }
            if (r.Trade != null)
            {
                r.Symptoms = Symptoms[r.Trade.Value].Where(s => sq.Contains(Squash(s))).ToList();
                r.Accessories = Acc[r.Trade.Value].Where(a => { var q = Squash(a); return sq.Contains("با" + q) || (sq.Contains(q) && !sq.Contains("بدون" + q)); }).ToList();
            }
            return r;
        }
    }
}
