﻿namespace BlazorShared
{
    public class BaseUrlConfiguration
    {
        public const string CONFIG_NAME = "baseUrls";

        public string ApiBase { get; set; }
        public string WebBase { get; set; }
        public string AdminBase { get; set; }
        public string OrderReserverFunc { get; set; }
        public string DeliveryProcessorFunc { get; set; }
    }
}
