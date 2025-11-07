using Microsoft.ML.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaitTimeTesting.Models
{
    public class Order
    {
        public Guid Uid { get; set; } = Guid.NewGuid();  // order ID
        public DateTimeOffset PlacedAt { get; set; }  // time order was placed
        public DateTimeOffset? CompletedAt { get; set; }  // time order was completed
        public string ItemIds { get; set; } = string.Empty;  // list of item ids in the order. Comma-separated like "1,6,3,7"
        public OrderStatus Status { get; set; } = OrderStatus.Pending; // status of the order (Pending, Complete)
        public string? PhoneNumber { get; set; }  // phone number of customer. mid-layer or payment should set this for us instead of giving userID and having us query it from DB
        public Double? EstimatedWaitTime { get; set; }  // estimated wait time for the order at time of placement
        public int PlaceInQueue { get; set; }  // position in the order queue at time of placement
        public NotificationPreference NotificationPref { get; set; } = NotificationPreference.None;  // customer preference for notifications set at order or saved on account
        [Column(TypeName = "nvarchar(255)")]
        public float[] ItemsAheadAtPlacement { get; set; } = new float[10]; // ML feature: number of each item ahead in the queue at placement time
        public float TotalItemsAheadAtPlacement { get; set; } // ML feature: total number of items ahead in the queue at placement time
    }

    public enum OrderStatus
    {
        Pending,
        Complete
    }

    public enum NotificationPreference // can add email stuff later
    {
        None,
        Sms
    }
}