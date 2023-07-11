namespace UserTaskChallenge.Models
{
    public class ReturnData
    {
        public Status status { get; set; }
        public dynamic data { get; set; }
        public List<string> errors { get; set; } = new List<string>();

        public enum Status
        {
            Ok = 1,
            Warning = 2,
            Error = 3
        }

    }


}
