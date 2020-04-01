using System;
using System.Security.Cryptography;
using System.Text;

namespace Api.ManagerGift.Entities
{
    public class User
    {
        public virtual Guid Id { get; set; }
        public virtual string UserName { get; set; }
        private  string _password { get; set; }
        public virtual Position Position { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual string  Email { get; set; }
        public virtual string FullName { get; set; }
        public virtual bool Status { get; set; }
        public virtual int MonthId { get; set; }
        public virtual int PermisionId { get; set; }
        public virtual bool IsUser { get; set; }

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
