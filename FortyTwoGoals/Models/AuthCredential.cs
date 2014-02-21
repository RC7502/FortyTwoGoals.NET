using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FortyTwoGoals.Models
{
    public class AuthCredential
    {
        public string AuthToken
        {
            get;
            set;
        }

        public string AuthTokenSecret
        {
            get;
            set;
        }

        public string UserId
        {
            get;
            set;
        }
    }
}
