using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;
using Facebook.Client;

namespace Squirrel
{
    public class SessionStorage
    {
        /// <summary> 
        /// Key used to store access token in app settings 
        /// </summary> 
        private const string AccessTokenSettingsKeyName = "fb_access_token";

        /// <summary> 
        /// Key used to store access token expiry in app settings 
        /// </summary> 
        private const string AccessTokenExpirySettingsKeyName = "fb_access_token_expiry";

        /// <summary> 
        /// Key used to state in app settings 
        /// </summary> 
        private const string StateSettingsKeyName = "fb_login_state";

        /// <summary> 
        /// Tries to retrieve a session 
        /// </summary> 
        /// <returns> 
        /// A valid login response with access token and expiry, or null (including if token already expired) 
        /// </returns> 
        public static FacebookSession Load()
        {
            // read access token 
            string accessTokenValue = LoadEncryptedSettingValue(AccessTokenSettingsKeyName);

            // read expiry 
            DateTime expiryValue = DateTime.MinValue;
            string expiryTicks = LoadEncryptedSettingValue(AccessTokenExpirySettingsKeyName);
            if (!string.IsNullOrWhiteSpace(expiryTicks))
            {
                long expiryTicksValue = 0;
                if (long.TryParse(expiryTicks, out expiryTicksValue))
                {
                    expiryValue = new DateTime(expiryTicksValue);
                }
            }

            // read state 
            string stateValue = LoadEncryptedSettingValue(StateSettingsKeyName);

            // return true only if both values retrieved and token not stale 
            if (!string.IsNullOrWhiteSpace(accessTokenValue) && expiryValue > DateTime.UtcNow)
            {
                return new FacebookSession()
                {
                    AccessToken = accessTokenValue,
                    Expires = expiryValue,
                    State = stateValue
                };
            }
            
            return null;
        }

        /// <summary> 
        /// Saves an access token an access token and its expiry 
        /// </summary> 
        /// <param name="session">A valid login response with access token and expiry</param> 
        public static void Save(FacebookSession session)
        {
            SaveEncryptedSettingValue(AccessTokenSettingsKeyName, session.AccessToken);
            SaveEncryptedSettingValue(AccessTokenExpirySettingsKeyName, session.Expires.Ticks.ToString());
            SaveEncryptedSettingValue(StateSettingsKeyName, session.State);
        }

        /// <summary> 
        /// Removes saved values for access token and expiry 
        /// </summary> 
        public static void Remove()
        {
            RemoveEncryptedSettingValue(AccessTokenSettingsKeyName);
            RemoveEncryptedSettingValue(AccessTokenExpirySettingsKeyName);
            RemoveEncryptedSettingValue(StateSettingsKeyName);
        }

        /// <summary> 
        /// Removes an encrypted setting value 
        /// </summary> 
        /// <param name="key">Key to remove</param> 
        private static void RemoveEncryptedSettingValue(string key)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(key);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        /// <summary> 
        /// Loads an encrypted setting value for a given key 
        /// </summary> 
        /// <param name="key">The key to load</param> 
        /// <returns> 
        /// The value of the key 
        /// </returns> 
        /// <exception cref="KeyNotFoundException">The given key was not found</exception> 
        private static string LoadEncryptedSettingValue(string key)
        {
            string value = null;
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                var protectedBytes = IsolatedStorageSettings.ApplicationSettings[key] as byte[];
                if (protectedBytes != null)
                {
                    byte[] valueBytes = ProtectedData.Unprotect(protectedBytes, null);
                    value = Encoding.UTF8.GetString(valueBytes, 0, valueBytes.Length);
                }
            }

            return value;
        }

        /// <summary> 
        /// Saves a setting value against a given key, encrypted 
        /// </summary> 
        /// <param name="key">The key to save against</param> 
        /// <param name="value">The value to save against</param> 
        /// <exception cref="System.ArgumentOutOfRangeException">The key or value provided is unexpected</exception> 
        private static void SaveEncryptedSettingValue(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value) && !string.IsNullOrEmpty(value))
            {
                byte[] valueBytes = Encoding.UTF8.GetBytes(value);

                // Encrypt the value by using the Protect() method. 
                byte[] protectedBytes = ProtectedData.Protect(valueBytes, null);
                if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
                {
                    IsolatedStorageSettings.ApplicationSettings[key] = protectedBytes;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings.Add(key, protectedBytes);
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            else
            {
                if (!string.IsNullOrEmpty(key) && key.Equals(StateSettingsKeyName))
                {
                    return;
                }

                throw new ArgumentOutOfRangeException();
            }
        }
    } 
}
