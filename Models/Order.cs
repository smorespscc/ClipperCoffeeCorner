using Microsoft.ML.Data;
using System;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using WaitTimeTesting.Options;

namespace WaitTimeTesting.Models
{
    public class Order
    {
        public Guid Uid { get; set; } = Guid.NewGuid();  // order ID
        public Guid CustomerId { get; set; }  // customer ID
        public DateTimeOffset PlacedAt { get; set; }  // time order was placed
        public DateTimeOffset? CompletedAt { get; set; }  // time order was completed
        public string ItemIds { get; set; } = string.Empty;  // list of item ids in the order. Comma-separated like "1,6,3,7"
        public OrderStatus Status { get; set; } = OrderStatus.Pending; // status of the order (Pending, Complete)
        public string? PhoneNumber { get; set; }  // phone number of customer. mid-layer or payment should set this for us instead of giving userID and having us query it from DB
        public string? Email { get; set; }  // email of customer. 
        public Double? EstimatedWaitTime { get; set; }  // estimated wait time for the order at time of placement
        public int PlaceInQueue { get; set; }  // position in the order queue at time of placement
        public NotificationPreference NotificationPref { get; set; } = NotificationPreference.None;  // customer preference for notifications set at order or saved on account
        public float[]? ItemsAheadAtPlacement { get; set; } = new float[Constants.MaxMenuId]; // ML feature: number of each item ahead in the queue at placement time
        public float TotalItemsAheadAtPlacement { get; set; } // ML feature: total number of items ahead in the queue at placement time
        public float? ActualWaitMinutes { get; set; }  // actual wait time in minutes (CompletedAt - PlacedAt) (might be useful for training idk)
        public float? PredictionError { get; set; } // error in prediction (|Actual - Estimated|) (might be useful for training idk)
    }

    public enum OrderStatus
    {
        Pending,
        Complete
    }

    [Flags]
    public enum NotificationPreference
    {
        None=0,
        Sms=1,
        Email=2,
        Both= Sms | Email
    }
}