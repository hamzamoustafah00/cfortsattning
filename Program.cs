using Microsoft.EntityFrameworkCore;
using System.Linq;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int? PostId { get; set; } 
    public List<Post> Posts { get; set; } = new List<Post>();
}


public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime PublishedOn { get; set; }
    public int BlogId { get; set; }
    public int UserId { get; set; }
    public Blog Blog { get; set; }
    public User User { get; set; }
}

public class Blog
{
    public int Id { get; set; }
    public string Url { get; set; }
    public string Name { get; set; }
    public List<Post> Posts { get; set; } = new List<Post>();
}

public class BlogDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=blog.db");
    }
}

class Program
{
    static void Main()
    {
        using (var context = new BlogDbContext())
        {
            context.Database.EnsureCreated();
        }

        using (var dbContext = new BlogDbContext())
        {
            var users = ReadTextFile<User>("user.txt", fields => new User
            {
                Id = int.Parse(fields[0]),
                Username = fields[1],
                Password = fields[2],
                PostId = int.Parse(fields[3]) 
            });

            var posts = ReadTextFile<Post>("post.txt", fields => new Post
            {
                Id = int.Parse(fields[0]),
                Title = fields[1],
                Content = fields[2],
                PublishedOn = DateTime.Parse(fields[3]),
                BlogId = int.Parse(fields[4]),
                UserId = int.Parse(fields[5]),
            });

            var blogs = ReadTextFile<Blog>("blog.txt", fields => new Blog
            {
                Id = int.Parse(fields[0]),
                Url = fields[1],
                Name = fields[2]
            });

            dbContext.Users.AddRange(users);
            dbContext.Posts.AddRange(posts);
            dbContext.Blogs.AddRange(blogs);
            dbContext.SaveChanges();
        }

        using (var dbContext = new BlogDbContext())
        {
            var userPosts = dbContext.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Blog)
                .ToList();

            foreach (var user in userPosts)
            {
                Console.WriteLine($"{user.Username}'s Posts:");
                foreach (var post in user.Posts)
                {
                    Console.WriteLine($"  {post.Title} - {post.Blog.Name}");
                }
                Console.WriteLine();
            }
        }
    }

    static List<T> ReadTextFile<T>(string filePath, Func<string[], T> createEntity)
    {
        return File.ReadAllLines(filePath)
            .Skip(1)
            .Select(line => line.Split(','))
            .Where(fields => fields.All(field => !string.IsNullOrWhiteSpace(field)))
            .Select(fields => createEntity(fields))
            .ToList();
    }
}
