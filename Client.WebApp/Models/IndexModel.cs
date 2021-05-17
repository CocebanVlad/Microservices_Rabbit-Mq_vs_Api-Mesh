using System;
using System.ComponentModel;

namespace Client.WebApp.Models
{
    public class IndexModel
    {
        [DisplayName("Input")]
        public string Input { get; set; }

        [DisplayName("API elapsed time")]
        public TimeSpan APIElapsedTime { get; set; }

        [DisplayName("API output")]
        public string APIOutput { get; set; }

        [DisplayName("MQ elapsed time")]
        public TimeSpan MQElapsedTime { get; set; }

        [DisplayName("MQ output")]
        public string MQOutput { get; set; }
    }
}