// Saman Windows | Majid Rastegarmoghaddam | Digi Service
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace Saman.UI
{
    public partial class MainWindow : Window
    {
        readonly Saman.Data.Db db = new Saman.Data.Db(Saman.Data.Db.DefaultPath());
        readonly WorkbenchViewModel vm;
        public MainWindow() { db.Migrate(); vm = new WorkbenchViewModel(db); InitializeComponent(); LaneList.ItemsSource = vm.Lanes; }
                void NewIntake_Click(object s, object e)
        {
            var w = new IntakeWindow(db) { Owner = this };
            if (w.ShowDialog() != true) return;
            var c = w.VM.ToCustomer(); var it = w.VM.ToIntake(db.NextNumber(), c);
            db.SaveIntake(c, it);
            vm.AddIntake(it, c.Name ?? c.Mobile);
            ReceiptPrinter.Print(db, it, c);
        }
        void Settings_Click(object s, RoutedEventArgs e) { new SettingsWindow(db) { Owner = this }.ShowDialog(); }
        void Card_Move(object s, MouseEventArgs e)
        {
            var b = (FrameworkElement)s;
            if (e.LeftButton == MouseButtonState.Pressed) DragDrop.DoDragDrop(b, b.Tag, DragDropEffects.Move);
        }
        void Lane_Drop(object s, DragEventArgs e)
        {
            var card = e.Data.GetData(typeof(Card)) as Card; var lane = ((FrameworkElement)s).Tag as Lane;
            if (card != null && lane != null) vm.Move(card, lane);
        }
    }
}
