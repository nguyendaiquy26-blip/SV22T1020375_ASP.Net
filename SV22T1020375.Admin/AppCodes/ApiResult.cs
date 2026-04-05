namespace SV22T1020375.Admin
{
    public class ApiResult
    {
        public ApiResult(int code, String message) 
        {
            Code = code;
            Message = message;
        }
        /// <summary>
        /// 0: Lỗi / hoặc không thành côngm ngược lại thành công
        /// </summary>
        public int Code {  get; set; }
        /// <summary>
        /// Thông báo lỗi (nếu có)
        /// </summary>
        public string Message { get; set; } = "";
    }
}
