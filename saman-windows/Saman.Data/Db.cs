// Saman Windows | Majid Rastegarmoghaddam | Digi Service
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper;
using Saman.Core.Models;
namespace Saman.Data
{
    /// Lite mode storage (SQLite). Express mode (SQL Server) will implement the same methods later.
    public class Db
    {
        readonly string cs;
        public static string DefaultPath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SamanSystem");
            Directory.CreateDirectory(dir); return Path.Combine(dir, "saman.db");
        }
        public Db(string path) { cs = "Data Source=" + path + ";Version=3;BinaryGUID=False;DateTimeKind=Utc;Foreign Keys=True"; }
        SQLiteConnection Open() { var c = new SQLiteConnection(cs); c.Open(); return c; }

        public void Migrate()
        {
            string sql;
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                Assembly.GetExecutingAssembly().GetManifestResourceNames().First(n => n.EndsWith("Schema_v1.sql"))))
            using (var r = new StreamReader(s)) sql = r.ReadToEnd();
            using (var c = Open()) { c.Execute("PRAGMA journal_mode=WAL;"); c.Execute(sql); }
        }

        public void SaveIntake(Customer cu, Intake it)
        {
            using (var c = Open()) using (var tx = c.BeginTransaction())
            {
                cu.UpdatedAtUtc = DateTime.UtcNow; it.UpdatedAtUtc = DateTime.UtcNow;
                var existing = string.IsNullOrWhiteSpace(cu.Mobile) ? null :
                    c.Query<Customer>("SELECT * FROM Customer WHERE Mobile=@Mobile AND IsDeleted=0 LIMIT 1", new { cu.Mobile }, tx).FirstOrDefault();
                if (existing != null) { cu.Id = existing.Id; c.Execute("UPDATE Customer SET Name=@Name,UpdatedAtUtc=@UpdatedAtUtc WHERE Id=@Id", cu, tx); }
                else c.Execute("INSERT INTO Customer(Id,Name,Mobile,UpdatedAtUtc,IsDeleted) VALUES(@Id,@Name,@Mobile,@UpdatedAtUtc,0)", cu, tx);
                it.CustomerId = cu.Id;
                c.Execute(@"INSERT INTO Intake(Id,Number,Trade,Stage,CustomerId,DeviceType,Brand,Model,Serial,Complaint,Accessories,EstimateRial,AdvanceRial,Technician,CreatedAtUtc,DeliveredAtUtc,UpdatedAtUtc,IsDeleted)
                            VALUES(@Id,@Number,@Trade,@Stage,@CustomerId,@DeviceType,@Brand,@Model,@Serial,@Complaint,@Accessories,@EstimateRial,@AdvanceRial,@Technician,@CreatedAtUtc,@DeliveredAtUtc,@UpdatedAtUtc,0)", it, tx);
                tx.Commit();
            }
        }
        /// Settings keys: ReceiptSize (A5|Thermal|Both), WarrantyDays (empty = not set), ShopName, ShopPhone (optional), TrackUrl (optional).
        public string GetSetting(string key, string def = "")
        { using (var c = Open()) { var v = c.ExecuteScalar<string>("SELECT Value FROM Settings WHERE Key=@key", new { key }); return v ?? def; } }
        public void SetSetting(string key, string value)
        { using (var c = Open()) c.Execute("INSERT OR REPLACE INTO Settings(Key,Value) VALUES(@key,@value)", new { key, value }); }
        public int NextNumber() { using (var c = Open()) return c.ExecuteScalar<int>("SELECT IFNULL(MAX(Number),1300)+1 FROM Intake"); }
        public void SetStage(Guid id, Stage stage)
        { using (var c = Open()) c.Execute("UPDATE Intake SET Stage=@s,UpdatedAtUtc=@u,DeliveredAtUtc=CASE WHEN @s=5 THEN @u ELSE NULL END WHERE Id=@id", new { s = (int)stage, u = DateTime.UtcNow, id }); }

        public class IntakeRow { public Intake Intake { get; set; } public string CustomerName { get; set; } }
        public List<IntakeRow> ActiveIntakes()
        {
            using (var c = Open())
                return c.Query<Intake, string, IntakeRow>(
                    @"SELECT i.*, IFNULL(NULLIF(cu.Name,''),cu.Mobile) AS CustomerName FROM Intake i LEFT JOIN Customer cu ON cu.Id=i.CustomerId
                      WHERE i.IsDeleted=0 AND i.Stage<5 ORDER BY i.Number DESC",
                    (i, n) => new IntakeRow { Intake = i, CustomerName = n }, splitOn: "CustomerName").ToList();
        }
        /// Previous repairs for the same serial/IMEI (used for the automatic repair-warranty hint).
        public List<Intake> History(string serial)
        {
            if (string.IsNullOrWhiteSpace(serial)) return new List<Intake>();
            using (var c = Open()) return c.Query<Intake>("SELECT * FROM Intake WHERE Serial=@serial AND IsDeleted=0 ORDER BY Number DESC", new { serial }).ToList();
        }
    }
}
