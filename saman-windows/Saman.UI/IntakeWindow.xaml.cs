// Saman Windows | Majid Rastegarmoghaddam | Digi Service
using System.Windows;
using Saman.Core.Models;
namespace Saman.UI
{
    public partial class IntakeWindow : Window
    {
        public IntakeViewModel VM { get; } = new IntakeViewModel();
        readonly Saman.Data.Db db;
        public IntakeWindow(Saman.Data.Db db) { this.db = db; InitializeComponent(); DataContext = VM; }
        void Serial_LostFocus(object s, RoutedEventArgs e) { VM.CheckHistory(db); }
        void Analyze_Click(object s, RoutedEventArgs e) { VM.Analyze(); }
        void Save_Click(object s, RoutedEventArgs e) { if (VM.Validate()) DialogResult = true; }
        void Cancel_Click(object s, RoutedEventArgs e) { DialogResult = false; }
    }
}
