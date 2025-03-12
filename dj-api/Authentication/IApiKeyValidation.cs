namespace dj_api.Authentication
{
    public interface IApiKeyValidation
    {
        bool IsValidApiKey(string userApiKey, string methode);

    }
    public class ApiKeyValidation : IApiKeyValidation
    {
        public bool IsValidApiKey(string apiKey,string methode)// later do methodes for specific key
        {
            if(apiKey == "274b8304-0a13-4351-8d40-0082da22a615")// Dj
            {
                return true;
            }
            else if (apiKey == "26d19db7-079c-4135-85bd-be8c90c5cbe0")// User
            {
                return true;
            }
            else if(apiKey == "688bf957-b5b4-439b-943a-42b3daf14507")// GuestUser
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
    }
}
