﻿using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Text;
using System.IO;
using System.Net;

namespace c_auth
{
    class c_api
    {
        private static string program_key { get; set; }
        private static string enc_key { get; set; }
        private static string iv_key { get; set; }

        private static string api_link = "https://firefra.me/auth/api/"; //maybe you'll make your own auth based on mine

        private static string user_agent = "Mozilla FireFrame"; //my ddos protection needs Mozilla in front

        private static string iv_input { get; set; }
        public static void c_init(string c_version, string c_program_key, string c_encryption_key)
        {
            try
            {
                if (c_encryption.ssl_cert(api_link, user_agent) != "oek2twC+UlJnvPdW/1eZPmTnKZUvDd4VsYcyGVOo5E0=")
                    Environment.Exit(0);

                using (var web = new WebClient())
                {
                    web.Headers["User-Agent"] = user_agent;
                    web.Proxy = null;

                    program_key = c_program_key;
                    iv_key = c_encryption.iv_key();
                    enc_key = c_encryption_key;

                    var values = new NameValueCollection();
                    values["version"] = c_encryption.encrypt(c_version, enc_key);
                    values["session_iv"] = c_encryption.encrypt(iv_key, enc_key);
                    values["api_version"] = c_encryption.encrypt("2.05b", enc_key);
                    values["program_key"] = c_encryption.base64_encode(program_key);

                    string result = Encoding.Default.GetString(web.UploadValues(api_link + "init.php", values));

                    if (result == "program_doesnt_exist")
                    {
                        MessageBox.Show("the program doesnt exist");
                        Environment.Exit(0);
                    }
                    else if (result == c_encryption.encrypt("wrong_version", enc_key))
                    {
                        MessageBox.Show("wrong program version");
                        Environment.Exit(0);
                    }
                    else if (result == c_encryption.encrypt("old_api_version", enc_key))
                    {
                        MessageBox.Show("please download the newest api version on the auth's website ");
                        Environment.Exit(0);
                    }
                    else if (c_encryption.decrypt(result, enc_key).Contains("started_program"))
                    {
                        string[] s = c_encryption.decrypt(result, enc_key).Split('|');
                        iv_input = s[1];
                    }
                    else
                    {
                        MessageBox.Show("invalid encryption key/iv or session expired");
                        Environment.Exit(0);
                    }
                }
            }
            catch (CryptographicException)
            {
                MessageBox.Show("invalid encryption key");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Environment.Exit(0);
            }
        }
        public static bool c_login(string c_username, string c_password, string c_hwid = "default")
        {
            if (c_hwid == "default") c_hwid = WindowsIdentity.GetCurrent().User.Value;

            try
            {
                using (var web = new WebClient())
                {
                    web.Headers["User-Agent"] = user_agent;
                    web.Proxy = null;

                    var values = new NameValueCollection();
                    values["username"] = c_encryption.encrypt(c_username, enc_key, iv_key);
                    values["password"] = c_encryption.encrypt(c_password, enc_key, iv_key);
                    values["hwid"] = c_encryption.encrypt(c_hwid, enc_key, iv_key);
                    values["iv_input"] = c_encryption.encrypt(iv_input, enc_key);
                    values["program_key"] = c_encryption.base64_encode(program_key);

                    string result = c_encryption.decrypt(Encoding.Default.GetString(web.UploadValues(api_link + "login.php", values)), enc_key, iv_key);

                    if (result == "invalid_username")
                    {
                        MessageBox.Show("invalid username");
                        return false;
                    }
                    else if (result == "invalid_password")
                    {
                        MessageBox.Show("invalid password");
                        return false;
                    }
                    else if (result == "no_sub")
                    {
                        MessageBox.Show("no sub");
                        return false;
                    }
                    else if (result == "invalid_hwid")
                    {
                        MessageBox.Show("invalid hwid");
                        return false;
                    }
                    else if (result.Contains("logged_in"))
                    {
                        string[] s = result.Split('|');

                        c_userdata.username = s[1];
                        c_userdata.email = s[2];
                        c_userdata.expires = c_encryption.unix_to_date(Convert.ToDouble(s[3]));
                        c_userdata.rank = Convert.ToInt32(s[4]);

                        shit_pass = c_password;

                        MessageBox.Show("logged in!");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("invalid encryption key/iv or session expired");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Environment.Exit(0);
                return false;
            }
        }
        public static bool c_register(string c_username, string c_email, string c_password, string c_token, string c_hwid = "default")
        {
            if (c_hwid == "default") c_hwid = WindowsIdentity.GetCurrent().User.Value;

            try
            {
                using (var web = new WebClient())
                {
                    web.Headers["User-Agent"] = user_agent;
                    web.Proxy = null;

                    var values = new NameValueCollection();
                    values["username"] = c_encryption.encrypt(c_username, enc_key, iv_key);
                    values["email"] = c_encryption.encrypt(c_email, enc_key, iv_key);
                    values["password"] = c_encryption.encrypt(c_password, enc_key, iv_key);
                    values["token"] = c_encryption.encrypt(c_token, enc_key, iv_key);
                    values["hwid"] = c_encryption.encrypt(c_hwid, enc_key, iv_key);
                    values["iv_input"] = c_encryption.encrypt(iv_input, enc_key);
                    values["program_key"] = c_encryption.base64_encode(program_key);

                    string result = c_encryption.decrypt(Encoding.Default.GetString(web.UploadValues(api_link + "register.php", values)), enc_key, iv_key);

                    if (result == "user_already_exists")
                    {
                        MessageBox.Show("user already exists");
                        return false;
                    }
                    else if (result == "email_already_exists")
                    {
                        MessageBox.Show("email already exists");
                        return false;
                    }
                    else if (result == "invalid_email_format")
                    {
                        MessageBox.Show("invalid email format");
                        return false;
                    }
                    else if (result == "invalid_token")
                    {
                        MessageBox.Show("invalid token");
                        return false;
                    }
                    else if (result == "maximum_users_reached")
                    {
                        MessageBox.Show("maximum users reached");
                        return false;
                    }
                    else if (result == "used_token")
                    {
                        MessageBox.Show("used token");
                        return false;
                    }
                    else if (result == "success")
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("invalid encryption key/iv or session expired");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Environment.Exit(0);
                return false;
            }
        }
        public static bool c_activate(string c_username, string c_password, string c_token)
        {
            try
            {
                using (var web = new WebClient())
                {
                    web.Headers["User-Agent"] = user_agent;
                    web.Proxy = null;

                    var values = new NameValueCollection();
                    values["username"] = c_encryption.encrypt(c_username, enc_key, iv_key);
                    values["password"] = c_encryption.encrypt(c_password, enc_key, iv_key);
                    values["token"] = c_encryption.encrypt(c_token, enc_key, iv_key);
                    values["iv_input"] = c_encryption.encrypt(iv_input, enc_key);
                    values["program_key"] = c_encryption.base64_encode(program_key);

                    string result = c_encryption.decrypt(Encoding.Default.GetString(web.UploadValues(api_link + "activate.php", values)), enc_key, iv_key);

                    if (result == "invalid_username")
                    {
                        MessageBox.Show("invalid username");
                        return false;
                    }
                    else if (result == "invalid_password")
                    {
                        MessageBox.Show("invalid password");
                        return false;
                    }
                    else if (result == "invalid_token")
                    {
                        MessageBox.Show("invalid token");
                        return false;
                    }
                    else if (result == "used_token")
                    {
                        MessageBox.Show("used token");
                        return false;
                    }
                    else if (result == "success")
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("invalid encryption key/iv or session expired");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Environment.Exit(0);
                return false;
            }
        }
        public static bool c_all_in_one(string c_token, string c_hwid = "default")
        {
            if (c_hwid == "default") c_hwid = WindowsIdentity.GetCurrent().User.Value;

            if (c_login(c_token, c_token, c_hwid))
                return true;

            else if (c_register(c_token, c_token + "@email.com", c_token, c_token, c_hwid))
            {
                MessageBox.Show("success, restarting...");
                Environment.Exit(0);
                return true;
            }

            return false;
        }
        private static string shit_pass { get; set; }
        public static string c_var(string c_var_name, string c_hwid = "default")
        {
            if (c_hwid == "default") c_hwid = WindowsIdentity.GetCurrent().User.Value;

            try
            {
                using (var web = new WebClient())
                {
                    web.Headers["User-Agent"] = user_agent;
                    web.Proxy = null;

                    var values = new NameValueCollection();
                    values["var_name"] = c_encryption.encrypt(c_var_name, enc_key, iv_key);
                    values["username"] = c_encryption.encrypt(c_userdata.username, enc_key, iv_key);
                    values["password"] = c_encryption.encrypt(shit_pass, enc_key, iv_key);
                    values["hwid"] = c_encryption.encrypt(c_hwid, enc_key, iv_key);
                    values["iv_input"] = c_encryption.encrypt(iv_input, enc_key);
                    values["program_key"] = c_encryption.base64_encode(program_key);

                    string result = c_encryption.decrypt(Encoding.Default.GetString(web.UploadValues(api_link + "var.php", values)), enc_key, iv_key);

                    return result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Environment.Exit(0);
                return "";
            }
        }
    }
    class c_userdata
    {
        public static string username { get; set; }
        public static string email { get; set; }
        public static DateTime expires { get; set; }
        public static int rank { get; set; }
    }
    class c_encryption
    {
        public static string base64_encode(string _) => System.Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(_));
        public static string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;

            byte[] aesKey = new byte[32];
            Array.Copy(key, 0, aesKey, 0, 32);
            encryptor.Key = aesKey;
            encryptor.IV = iv;

            MemoryStream memoryStream = new MemoryStream();

            ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();

            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);

            byte[] plainBytes = Encoding.Default.GetBytes(plainText);

            cryptoStream.Write(plainBytes, 0, plainBytes.Length);

            cryptoStream.FlushFinalBlock();

            byte[] cipherBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);

            return cipherText;
        }

        public static string DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;

            byte[] aesKey = new byte[32];
            Array.Copy(key, 0, aesKey, 0, 32);
            encryptor.Key = aesKey;
            encryptor.IV = iv;

            MemoryStream memoryStream = new MemoryStream();

            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

            string plainText = String.Empty;
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                cryptoStream.FlushFinalBlock();

                byte[] plainBytes = memoryStream.ToArray();

                plainText = Encoding.Default.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }
            return plainText;
        }
        public static string iv_key() => Guid.NewGuid().ToString().Substring(0, Guid.NewGuid().ToString().IndexOf("-", StringComparison.Ordinal));
        public static string encrypt(string message, string enc_key, string iv = "default_iv")
        {
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.Default.GetBytes(enc_key));

            if (iv == "default_iv")
            {
                byte[] iv_b = new byte[16] { 0x1, 0x5, 0x1, 0x4, 0x8, 0x3, 0x4, 0x6, 0x2, 0x6, 0x5, 0x7, 0x8, 0x3, 0x9, 0x4 };

                return EncryptString(message, key, iv_b);
            }
            else
            {
                byte[] iv_b = Encoding.Default.GetBytes(Convert.ToBase64String(mySHA256.ComputeHash(Encoding.Default.GetBytes(iv))).Substring(0, 16));

                return EncryptString(message, key, iv_b);
            }
        }

        public static string decrypt(string message, string enc_key, string iv = "default_iv")
        {
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.Default.GetBytes(enc_key));

            if (iv == "default_iv")
            {
                byte[] iv_b = new byte[16] { 0x1, 0x5, 0x1, 0x4, 0x8, 0x3, 0x4, 0x6, 0x2, 0x6, 0x5, 0x7, 0x8, 0x3, 0x9, 0x4 };

                return DecryptString(message, key, iv_b);
            }
            else
            {
                byte[] iv_b = Encoding.Default.GetBytes(Convert.ToBase64String(mySHA256.ComputeHash(Encoding.Default.GetBytes(iv))).Substring(0, 16));

                return DecryptString(message, key, iv_b);
            }
        }

        public static DateTime unix_to_date(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static string ssl_cert(string url, string user_agent)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = user_agent;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Close();

            X509Certificate cert = request.ServicePoint.Certificate;
            X509Certificate2 cert2 = new X509Certificate2(cert);

            SHA256 x = SHA256Managed.Create();
            byte[] retUrn = x.ComputeHash(Encoding.Default.GetBytes(cert2.GetPublicKeyString()));

            return Convert.ToBase64String(retUrn);
        }
    }
}