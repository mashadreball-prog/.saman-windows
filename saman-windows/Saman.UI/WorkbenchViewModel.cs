// Saman Windows | Majid Rastegarmoghaddam | Digi Service
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Saman.Core.Models;
namespace Saman.UI
{
    public class Card : INotifyPropertyChanged
    {
        public Guid Id { get; set; } public string Number { get; set; } public string Title { get; set; } public string Customer { get; set; }
        public Trade Trade { get; set; } public int AgeDays { get; set; }
        Stage _stage; public Stage Stage { get { return _stage; } set { _stage = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Stage")); } }
        public string AgeBrushKey { get { return AgeDays > 7 ? "Err" : AgeDays > 3 ? "Warn" : "Ok"; } }
        public string TradeBrushKey { get { return "Trade" + Trade; } }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class Lane { public string Title { get; set; } public Stage Stage { get; set; } public ObservableCollection<Card> Cards { get; set; } = new ObservableCollection<Card>(); }
    public class WorkbenchViewModel
    {
        public ObservableCollection<Lane> Lanes { get; } = new ObservableCollection<Lane>();
        readonly Saman.Data.Db db;
        public WorkbenchViewModel(Saman.Data.Db db)
        {
            var names = new[] { "ورودی و عیب‌یابی", "منتظر تأیید مشتری", "منتظر قطعه", "روی میز تعمیر", "آماده تحویل" };
            var st = new[] { Stage.Intake, Stage.AwaitApproval, Stage.AwaitPart, Stage.OnBench, Stage.Ready };
            this.db = db;
            for (int i = 0; i < 5; i++) Lanes.Add(new Lane { Title = names[i], Stage = st[i] });
            foreach (var r in db.ActiveIntakes()) AddCard(r.Intake, r.CustomerName, false);
        }
        public void AddIntake(Saman.Core.Models.Intake it, string customer) { AddCard(it, customer, true); }
        void AddCard(Saman.Core.Models.Intake it, string customer, bool top)
        {
            var title = ((it.Brand ?? "") + " " + (it.DeviceType ?? "") + " " + (it.Model ?? "")).Trim();
            var card = new Card { Id = it.Id, Number = it.Number.ToString(), Title = title, Customer = customer, Trade = it.Trade, Stage = it.Stage, AgeDays = (int)(DateTime.UtcNow - it.CreatedAtUtc).TotalDays };
            var lane = Lanes.First(l => l.Stage == it.Stage);
            if (top) lane.Cards.Insert(0, card); else lane.Cards.Add(card);
        }
        public void Move(Card card, Lane to)
        {
            var from = Lanes.First(l => l.Cards.Contains(card)); if (from == to) return;
            from.Cards.Remove(card); card.Stage = to.Stage; to.Cards.Add(card);
            db.SetStage(card.Id, to.Stage);
            // TODO: send "ready" SMS when to.Stage == Stage.Ready
        }
    }
}
