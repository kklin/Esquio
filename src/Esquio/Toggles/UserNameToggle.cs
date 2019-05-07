using Esquio.Abstractions;
using Esquio.Abstractions.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Esquio.Toggles
{
    [DesignTypeParameter(ParameterName = Users, ParameterType = "System.String", ParameterDescription = "The collection of user(s) to activate this toggle separated by ';' character")]
    public class UserNameToggle
        : IToggle
    {
        const string SPLIT_SEPARATOR = ";";
        const string Users = nameof(Users);
        private readonly IUserNameProviderService _userNameProviderService;
        private readonly IFeatureStore _featureStore;

        public UserNameToggle(IUserNameProviderService userNameProviderService, IFeatureStore featureStore)
        {
            _userNameProviderService = userNameProviderService ?? throw new ArgumentNullException(nameof(userNameProviderService));
            _featureStore = featureStore ?? throw new ArgumentNullException(nameof(featureStore));
        }
        public async Task<bool> IsActiveAsync(string featureName, string applicationName = null)
        {
            var currentUserName = await _userNameProviderService
                .GetCurrentUserNameAsync();

            if (currentUserName != null)
            {
                var feature = await _featureStore.FindFeatureAsync(featureName, applicationName);
                var toggle = feature.GetToggle(this.GetType().FullName);
                var activeUserNames = toggle.GetParameterValue<string>(Users);

                if (activeUserNames != null &&
                    activeUserNames.Split(SPLIT_SEPARATOR).Contains(currentUserName, StringComparer.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}