using System;
using System.Security.Cryptography;
using System.Text;

namespace Api.ManagerGift.Entities
{
    public class UserLogPassword
    {
        public virtual Guid Id { get; set; }
        public virtual Guid UserId { get; set; }
        private  string _password { get; set; }
        public virtual int Time { get; set; }
        public virtual DateTime CreateDate { get; set; }

        public virtual string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value != null ? value.Length == 128 ? value : ChangeSha512(value) : null;
            }
        }

        public static string ChangeSha512(string pass)
        {
            var result = new StringBuilder();
            var encode = new SHA512CryptoServiceProvider();
            byte[] arrByte = Encoding.UTF8.GetBytes(pass);
            arrByte = encode.ComputeHash(arrByte);
            foreach (byte item in arrByte)
                result.Append(item.ToString("x2").ToLower());
            return result.ToString();
        }
    }
}
