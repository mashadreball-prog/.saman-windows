// Saman Windows | Majid Rastegarmoghaddam | Digi Service
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Saman.Core.Models;
using Saman.Core.Services;
namespace Saman.UI
{
    public class TradeItem { public Trade Value { get; set; } public string Title { get; set; } }
    public class IntakeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void Set<T>(ref T f, T v, [CallerMemberName] string n = null) { f = v; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(n)); }
        readonly IntakeParser parser = new IntakeParser();

        public List<TradeItem> Trades { get; } = new List<TradeItem> {
            new TradeItem{Value=Trade.Mobile,Title="موبایل"}, new TradeItem{Value=Trade.AudioVideo,Title="صوتی و تصویری"},
            new TradeItem{Value=Trade.Appliance,Title="لوازم خانگی"}, new TradeItem{Value=Trade.Car,Title="الکترونیک خودرو"},
            new TradeItem{Value=Trade.Computer,Title="کامپیوتر و لپ‌تاپ"}, new TradeItem{Value=Trade.Board,Title="برد صنعتی"} };

        string smart, type, brand, model, serial, name, phone, complaint, accessories, est, adv, status;
        Trade trade = Trade.Mobile;
        public string SmartText { get { return smart; } set { Set(ref smart, value); } }
        public Trade Trade { get { return trade; } set { Set(ref trade, value); } }
        public string DeviceType { get { return type; } set { Set(ref type, value); } }
        public string Brand { get { return brand; } set { Set(ref brand, value); } }
        public string Model { get { return model; } set { Set(ref model, value); } }
        public string Serial { get { return serial; } set { Set(ref serial, value); } }
        public string CustomerName { get { return name; } set { Set(ref name, value); } }
        public string Phone { get { return phone; } set { Set(ref phone, value); } }
        public string Complaint { get { return complaint; } set { Set(ref complaint, value); } }
        public string Accessories { get { return accessories; } set { Set(ref accessories, value); } }
        public string EstimateToman { get { return est; } set { Set(ref est, value); } }
        public string AdvanceToman { get { return adv; } set { Set(ref adv, value); } }
        public string Status { get { return status; } set { Set(ref status, value); } }

        public string HistoryNotice { get; private set; }
        public bool HasHistory { get; private set; }
        public bool InWarranty { get; private set; }
        void Raise(string n) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(n)); }
        /// Looks up earlier repairs of the same serial/IMEI. Counts days from delivery (or from intake if not delivered yet).
        public void CheckHistory(Saman.Data.Db db)
        {
            var list = db.History(Serial);
            HasHistory = list.Count > 0; InWarranty = false; HistoryNotice = null;
            if (HasHistory)
            {
                var last = list[0]; int days = (int)(System.DateTime.UtcNow - (last.DeliveredAtUtc ?? last.CreatedAtUtc)).TotalDays; int w;
                HistoryNotice = "سابقه تعمیر: این دستگاه " + ReceiptPrinter.Fa(days.ToString()) + " روز پیش (پذیرش #" + ReceiptPrinter.Fa(last.Number.ToString()) + ") ثبت شده است.";
                if (int.TryParse(db.GetSetting("WarrantyDays", ""), out w) && w > 0 && days <= w) InWarranty = true;
            }
            Raise("HistoryNotice"); Raise("HasHistory"); Raise("InWarranty");
        }
        public void Analyze()
        {
            var r = parser.Parse(SmartText);
            int n = 0;
            if (r.Trade != null) { Trade = r.Trade.Value; n++; }
            if (r.Type != null) { DeviceType = r.Type; n++; }
            if (r.Brand != null) { Brand = r.Brand; n++; }
            var model = string.Join(" ", new[] { r.Size != null ? r.Size + " اینچ" : null, r.Model }.Where(x => x != null));
            if (model.Length > 0) { Model = model; n++; }
            if (r.Name != null) { CustomerName = r.Name; n++; }
            if (r.Phone != null) { Phone = r.Phone; n++; }
            if (r.Symptoms.Count > 0) { Complaint = string.Join("، ", r.Symptoms); n++; }
            if (r.Accessories.Count > 0) { Accessories = string.Join("، ", r.Accessories); n++; }
            if (r.Estimate > 0) { EstimateToman = r.Estimate.ToString(); n++; }
            if (r.Advance > 0) { AdvanceToman = r.Advance.ToString(); n++; }
            Status = n + " مورد تشخیص داده شد. لطفاً قبل از ثبت بررسی کنید.";
        }
        static long ToRial(string toman) { long v; return long.TryParse(toman, out v) ? v * 10 : 0; }
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(CustomerName) && string.IsNullOrWhiteSpace(Phone)) { Status = "نام یا شماره مشتری را وارد کنید."; return false; }
            if (string.IsNullOrWhiteSpace(DeviceType) && string.IsNullOrWhiteSpace(Brand)) { Status = "نوع یا برند دستگاه را وارد کنید."; return false; }
            return true;
        }
        public Intake ToIntake(int number, Customer c)
        {
            return new Intake { Number = number, Trade = Trade, Stage = Stage.Intake, CustomerId = c.Id, DeviceType = DeviceType, Brand = Brand, Model = Model,
                Serial = Serial, Complaint = Complaint, Accessories = Accessories, EstimateRial = ToRial(EstimateToman), AdvanceRial = ToRial(AdvanceToman) };
        }
        public Customer ToCustomer() { return new Customer { Name = CustomerName, Mobile = Phone }; }
    }
}
