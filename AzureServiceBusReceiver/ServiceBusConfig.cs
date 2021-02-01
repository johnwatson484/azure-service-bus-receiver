namespace AzureServiceBusSender
{
    class ServiceBusConfig
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Queue { get; set; }
        public string Topic { get; set; }
        public string Subscription { get; set; }
    }
}
