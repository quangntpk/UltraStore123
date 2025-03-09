

namespace UltraStrore.Helper
{
    public class APIResponse
    {
        internal bool IsSuccess;

        public int ResponseCode {  get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
        public object Data { get; set; }
    }
}
