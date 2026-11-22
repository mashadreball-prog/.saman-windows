// Saman Windows | Majid Rastegarmoghaddam | Digi Service
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using QRCoder;
using Saman.Core.Models;
using Saman.Data;
namespace Saman.UI
{
    public static class ReceiptPrinter
    {
        static readonly Brush Pcb = new SolidColorBrush(Color.FromRgb(0x0D, 0x4A, 0x40));
        static readonly Brush Copper = new SolidColorBrush(Color.FromRgb(0xC9, 0x70, 0x2A));
        static readonly Brush Grey = new SolidColorBrush(Color.FromRgb(0x4A, 0x5A, 0x56));

        public static string Fa(string s) { return new string((s ?? "").Select(ch => ch >= '0' && ch <= '9' ? (char)('۰' + (ch - '0')) : ch).ToArray()); }
        static string Money(long rial) { return Fa((rial / 10).ToString("N0", CultureInfo.InvariantCulture).Replace(",", "٬")) + " تومان"; }
        public static string JalaliNow()
        {
            var pc = new PersianCalendar(); var d = DateTime.Now;
            return Fa(string.Format("{0}/{1:00}/{2:00} - {3:00}:{4:00}", pc.GetYear(d), pc.GetMonth(d), pc.GetDayOfMonth(d), d.Hour, d.Minute));
        }

        public static void Print(Db db, Intake it, Customer c)
        {
            var size = db.GetSetting("ReceiptSize", "A5");
            var pd = new PrintDialog(); if (pd.ShowDialog() != true) return;
            if (size == "A5" || size == "Both") Send(pd, Build(db, it, c, false));
            if (size == "Thermal" || size == "Both") Send(pd, Build(db, it, c, true));
        }
        static void Send(PrintDialog pd, FlowDocument doc) { pd.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "رسید پذیرش"); }

        static Paragraph P(string text, double size, Brush fg = null, bool bold = false, TextAlignment al = TextAlignment.Right)
        { return new Paragraph(new Run(text)) { FontSize = size, Foreground = fg ?? Brushes.Black, FontWeight = bold ? FontWeights.Bold : FontWeights.Normal, Margin = new Thickness(0, 2, 0, 2), TextAlignment = al }; }

        static void Row(TableRowGroup g, string k, string v, double fs)
        {
            if (string.IsNullOrWhiteSpace(v)) return;
            var r = new TableRow();
            r.Cells.Add(new TableCell(P(k, fs, Grey)) { Padding = new Thickness(0, 3, 0, 3), BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0, 1) });
            r.Cells.Add(new TableCell(P(v, fs)) { Padding = new Thickness(0, 3, 0, 3), BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0, 1) });
            g.Rows.Add(r);
        }

        public static FlowDocument Build(Db db, Intake it, Customer c, bool thermal)
        {
            double fs = thermal ? 11 : 13;
            var doc = new FlowDocument { FlowDirection = FlowDirection.RightToLeft, FontFamily = new FontFamily("Vazirmatn, Tahoma"),
                PageWidth = thermal ? 302 : 559, PageHeight = thermal ? 900 : 794, PagePadding = new Thickness(thermal ? 10 : 36), ColumnWidth = 10000 };
            var shop = db.GetSetting("ShopName", "دیجی سرویس"); var phone = db.GetSetting("ShopPhone", ""); var url = db.GetSetting("TrackUrl", "");
            doc.Blocks.Add(P(shop, fs + 6, Pcb, true));
            doc.Blocks.Add(P("رسید پذیرش دستگاه   #" + Fa(it.Number.ToString()), fs + 3, Copper, true));
            if (!string.IsNullOrWhiteSpace(phone)) doc.Blocks.Add(P("تلفن: " + Fa(phone), fs, Grey));
            var t = new Table { CellSpacing = 0 }; t.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); t.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) });
            var g = new TableRowGroup(); t.RowGroups.Add(g);
            Row(g, "تاریخ", JalaliNow(), fs);
            Row(g, "مشتری", Fa(((c.Name ?? "") + " " + (c.Mobile ?? "")).Trim()), fs);
            Row(g, "نوع و برند", ((it.DeviceType ?? "") + " " + (it.Brand ?? "")).Trim(), fs);
            Row(g, "مدل", it.Model, fs); Row(g, "IMEI / سریال", it.Serial, fs);
            Row(g, "ایراد اعلام‌شده", it.Complaint, fs); Row(g, "لوازم همراه", it.Accessories, fs);
            if (it.EstimateRial > 0) Row(g, "برآورد", Money(it.EstimateRial), fs);
            if (it.AdvanceRial > 0) Row(g, "پیش‌دریافت", Money(it.AdvanceRial), fs);
            doc.Blocks.Add(t);
            var qrText = string.IsNullOrWhiteSpace(url) ? "R-" + it.Number : url.TrimEnd('/') + "/" + it.Number;
            var qr = new Image { Source = QrImage(qrText), Width = thermal ? 90 : 100, Height = thermal ? 90 : 100, HorizontalAlignment = HorizontalAlignment.Right };
            doc.Blocks.Add(new BlockUIContainer(qr) { Margin = new Thickness(0, 10, 0, 2) });
            doc.Blocks.Add(P(string.IsNullOrWhiteSpace(url) ? "این رسید هنگام تحویل دستگاه لازم است. شماره پذیرش: " + Fa(it.Number.ToString()) : "با اسکن این کد وضعیت تعمیر را ببینید. این رسید هنگام تحویل دستگاه لازم است.", fs - 1, Grey));
            doc.Blocks.Add(P("امضای مشتری: ........................        امضای پذیرش‌گر: ........................", fs - 1, Grey));
            return doc;
        }

        public static ImageSource QrImage(string text)
        {
            var data = new QRCodeGenerator().CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
            var m = data.ModuleMatrix; int n = m.Count; var gg = new GeometryGroup();
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++) if (m[y][x]) gg.Children.Add(new RectangleGeometry(new Rect(x, y, 1.02, 1.02)));
            var bg = new GeometryDrawing(Brushes.White, null, new RectangleGeometry(new Rect(-2, -2, n + 4, n + 4)));
            var fg = new GeometryDrawing(Brushes.Black, null, gg);
            var dg = new DrawingGroup(); dg.Children.Add(bg); dg.Children.Add(fg);
            var img = new DrawingImage(dg); img.Freeze(); return img;
        }
    }
}
