// Saman Windows | Majid Rastegarmoghaddam | Digi Service
using System.Windows;
using Saman.Data;
namespace Saman.UI
{
    public partial class SettingsWindow : Window
    {
        readonly Db db;
        public SettingsWindow(Db db)
        {
            this.db = db; InitializeComponent();
            var size = db.GetSetting("ReceiptSize", "A5");
            RbA5.IsChecked = size == "A5"; RbThermal.IsChecked = size == "Thermal"; RbBoth.IsChecked = size == "Both";
            TbShop.Text = db.GetSetting("ShopName", "دیجی سرویس"); TbPhone.Text = db.GetSetting("ShopPhone", "");
            TbUrl.Text = db.GetSetting("TrackUrl", ""); TbWarranty.Text = db.GetSetting("WarrantyDays", "");
        }
        void Save_Click(object s, RoutedEventArgs e)
        {
            int w;
            if (!string.IsNullOrWhiteSpace(TbWarranty.Text) && (!int.TryParse(TbWarranty.Text.Trim(), out w) || w < 0)) { Msg.Text = "مدت گارانتی باید عدد صحیح باشد."; return; }
            db.SetSetting("ReceiptSize", RbThermal.IsChecked == true ? "Thermal" : RbBoth.IsChecked == true ? "Both" : "A5");
            db.SetSetting("ShopName", TbShop.Text.Trim()); db.SetSetting("ShopPhone", TbPhone.Text.Trim());
            db.SetSetting("TrackUrl", TbUrl.Text.Trim()); db.SetSetting("WarrantyDays", TbWarranty.Text.Trim());
            Msg.Text = "ذخیره شد.";
        }
    }
}
