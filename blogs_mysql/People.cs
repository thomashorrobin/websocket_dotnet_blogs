using System;
using System.Collections.Generic;

namespace blogs_mysql
{
    public partial class People
    {
        public People()
        {
            Blogposts = new HashSet<Blogposts>();
        }

        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<Blogposts> Blogposts { get; set; }
    }
}
