using Microsoft.EntityFrameworkCore;
using System;

namespace MIT.EntityFrameworkCore
{
    public class GPAYDbContext : DbContext
    {
        public GPAYDbContext(DbContextOptions<GPAYDbContext> options)
           : base(options)
        {

        }

    }
}
