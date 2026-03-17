using System.Collections.Generic;
using System.Linq;
using Proto;

namespace Globals
{
    public static class FeatureManager
    {
        public static bool IsFeatureAllowed(FeatureName featureName)
        {
            // if (User.UserAccount == null)
            // {
            //     return false;
            // }
            //
            // if (User.UserAccount.AllowedFeatures == null || User.UserAccount.AllowedFeatures.Count == 0)
            // {
            //     return false;
            // }
            //
            // return User.UserAccount.AllowedFeatures.Contains(featureName);
            return true;
        }
        
        public static bool IsFeatureAllowed(int featureValue)
        {
            return IsFeatureAllowed((FeatureName)featureValue);
        }
        
        
        // public static List<FeatureName> GetAllowedFeatures()
        // {
        //     if (User.UserAccount == null || User.UserAccount.AllowedFeatures == null)
        //     {
        //         return new List<FeatureName>();
        //     }
        //
        //     return new List<FeatureName>(User.UserAccount.AllowedFeatures);
        // }
        //
        // public static bool HasAnyAllowedFeature()
        // {
        //     if (User.UserAccount == null || User.UserAccount.AllowedFeatures == null)
        //     {
        //         return false;
        //     }
        //
        //     return User.UserAccount.AllowedFeatures.Count > 0;
        // }
    }
}

