namespace HNOne.Web.Commons
{
    public static class GlobalContants
    {
        public static int[] PageSizeList = [10, 50, 100, 300, 500, 1000];
        public static int PageSize = 50;
        public static int PageSizeDefault = 5000;
        public static DateTime MIN_DATE = new DateTime(2024, 01, 01);
        public const string FORMAT_GRID_DISPLAYTEXT_COUNT = "Số dòng: {0}";
        public const string FORMAT_GRID_DISPLAYTEXT_SUM = "Tổng: {0}";
        public const string FORMAT_GRID_DISPLAYTEXT_SUM_PAYROLL = "{0}";
        public const string FORMAT_NUMBER = "#,###0.##";//
        public const string FORMAT_NUMBER_DOUBLE_2PLACE = "n2";//
        public const string FORMAT_NUMBER_DOUBLE_1PLACE = "n1";//
        public const string FORMAT_CURRENCY = "###,###,###,##0.###";//
        public const string FORMAT_DATE = "dd/MM/yyyy";
        public const string FORMAT_MONTH = "MM/yyyy";
        public const string FORMAT_TIME = "HH:mm";
        public const string FORMAT_DATE_TIME = "dd/MM/yyyy HH:mm";
        public const string FORMAT_DAY = "dd/MM";
        public const string ENUM_CONTRACT_NO = "CONTRACT_NO";
        public const string CONTRACT_APPENDIX_NO = "CONTRACT_APPENDIX_NO";
        public const string ENUM_REASON_DNNP = "DNNP";
        public const string ENUM_REASON_DNTC = "DNTC";
        public const string ENUM_REASON_CTQD = "CTQD";
        public const string ENUM_YES = "Y";
        public const string ENUM_NO = "N";
        public const string FORMAT_GRID_DISPLAYTEXT_SUM_HOUR = "Tổng: {0} giờ";
        public const string FORMAT_GRID_DISPLAYTEXT_SUM_DAY = "Tổng: {0} ngày";

        public const string MIME_TYPE_WORD = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    }
}
