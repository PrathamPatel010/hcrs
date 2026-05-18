namespace HCRS.RestApi.Extensions
{
    public static class ControllerExtension
    {
        public static void ConfigureControllers(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
        }
    }
}
