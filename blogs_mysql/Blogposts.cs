using System;
using System.Collections.Generic;

namespace blogs_mysql
{
    public partial class Blogposts
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime DatePosted { get; set; }
        public Guid AuthorId { get; set; }
        public Guid BlogId { get; set; }

        public People Author { get; set; }
        public Blogs Blog { get; set; }
    }
}
