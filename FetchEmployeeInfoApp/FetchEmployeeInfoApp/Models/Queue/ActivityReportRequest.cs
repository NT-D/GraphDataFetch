﻿namespace FetchEmployeeInfoApp.Models.Queue
{
    public class ActivityReportRequest
    {
        public ReportType Type { get; set; }

        //Please take care about to change this property name because we use json payload binding
        //https://docs.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings#binding-expressions---json-payloads
        public string TypeString { get; set; }

        public ReportPeriod Period { get; set; }

        public ActivityReportRequest() { }

        public ActivityReportRequest(ReportType type, ReportPeriod period)
        {
            Type = type;
            TypeString = type.ToString("g");
            Period = period;
        }
    }

    public enum ReportType
    {
        Sfb,
        Exo,
        OneDrive,
        Teams
    }

    public enum ReportPeriod
    {
        D7,
        D30,
        D90,
        D180
    }
}