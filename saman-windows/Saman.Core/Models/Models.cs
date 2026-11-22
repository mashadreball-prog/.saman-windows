// Saman Windows | Majid Rastegarmoghaddam | Digi Service
using System;
namespace Saman.Core.Models
{
    public enum Trade { Mobile, AudioVideo, Appliance, Car, Computer, Board }
    public enum Stage { Intake, AwaitApproval, AwaitPart, OnBench, Ready, Delivered }

    public abstract class Entity
    {
        public Guid Id { get; set; } = Guid.NewGuid();      // GUID for Android sync
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }                  // soft delete
    }
    public class Customer : Entity { public string Name { get; set; } public string Mobile { get; set; } }
    public class Intake : Entity
    {
        public int Number { get; set; }
        public Trade Trade { get; set; }
        public Stage Stage { get; set; }
        public Guid CustomerId { get; set; }
        public string DeviceType { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }                   // IMEI / VIN / serial
        public string Complaint { get; set; }
        public string Accessories { get; set; }
        public long EstimateRial { get; set; }               // integer money, never double
        public long AdvanceRial { get; set; }
        public string Technician { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAtUtc { get; set; }
    }
    public class BoardPin : Entity { public Guid IntakeId { get; set; } public double X { get; set; } public double Y { get; set; } public int Kind { get; set; } public string Note { get; set; } }
}
