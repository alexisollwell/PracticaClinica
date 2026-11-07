namespace ClinicaMedica.Authentication
{
    public interface IApiKeyValidator
    {
        Task<bool> ValidateApiKeyAsync(string apiKey);
    }
}

