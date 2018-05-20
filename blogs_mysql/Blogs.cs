using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace blogs_mysql
{
    public partial class Blogs
    {
        public Blogs()
        {
            Blogposts = new HashSet<Blogposts>();
        }

		public Guid Id { get; set; }
        public string Name { get; set; }

		[JsonIgnore]
        public ICollection<Blogposts> Blogposts { get; set; }
    }
}
