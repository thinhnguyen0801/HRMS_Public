namespace HNOne.API.Configurations
{
    public class JwtConfiguration
    {
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
        public string JwtSecurityKey { get; set; }
        public int JwtExpiryInDays { get; set; }
        public int JwtRefreshTokenExpiryInDays { get; set; }

        public JwtConfiguration()
        {
            JwtIssuer = string.Empty;
            JwtAudience = string.Empty;
            JwtSecurityKey = string.Empty;
        }
    }
}
