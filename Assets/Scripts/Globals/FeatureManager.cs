using System.Collections.Generic;
using System.Linq;
using Proto;

namespace Globals
{
    public static class FeatureManager
    {
        public static bool IsFeatureAllowed(FeatureName featureName)
        {
            if (User.userProfile == null)
            {
                return false;
            }

            if (User.userProfile.AllowedFeatures == null || User.userProfile.AllowedFeatures.Count == 0)
            {
                return false;
            }

            return User.userProfile.AllowedFeatures.Contains(featureName);
        }
        
        public static bool IsFeatureAllowed(int featureValue)
        {
            return IsFeatureAllowed((FeatureName)featureValue);
        }
        
        
        public static List<FeatureName> GetAllowedFeatures()
        {
            if (User.userProfile == null || User.userProfile.AllowedFeatures == null)
            {
                return new List<FeatureName>();
            }

            return new List<FeatureName>(User.userProfile.AllowedFeatures);
        }
        
        public static bool HasAnyAllowedFeature()
        {
            if (User.userProfile == null || User.userProfile.AllowedFeatures == null)
            {
                return false;
            }

            return User.userProfile.AllowedFeatures.Count > 0;
        }
    }
}

