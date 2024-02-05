using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
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

public class BlogDbContext
{
    public List<User> Users { get; set; } = new List<User>();
    public List<Post> Posts { get; set; } = new List<Post>();
    public List<Blog> Blogs { get; set; } = new List<Blog>();
}

class Program
{
    static void Main()
    {
        var dbContext = new BlogDbContext();

        // Read data from text files
        ReadTextFile("user.txt", fields => dbContext.Users.Add(new User
        {
            Id = int.Parse(fields[0]),
            Username = fields[1],
            Password = fields[2],
        }));

        ReadTextFile("post.txt", fields => dbContext.Posts.Add(new Post
        {
            Id = int.Parse(fields[0]),
            Title = fields[1],
            Content = fields[2],
            PublishedOn = DateTime.Parse(fields[3]),
            BlogId = int.Parse(fields[4]),
            UserId = int.Parse(fields[5]),
        }));

        ReadTextFile("blog.txt", fields => dbContext.Blogs.Add(new Blog
        {
            Id = int.Parse(fields[0]),
            Url = fields[1],
            Name = fields[2]
        }));

        // Associate posts with blogs and users
        foreach (var post in dbContext.Posts)
        {
            post.Blog = dbContext.Blogs.Single(b => b.Id == post.BlogId);
            post.User = dbContext.Users.Single(u => u.Id == post.UserId);
            post.Blog.Posts.Add(post);
            post.User.Posts.Add(post);
        }

        // Perform database-like operations in-memory
        var userPosts = dbContext.Users
            .Select(u => new
            {
                User = u,
                Posts = u.Posts.Join(dbContext.Posts,
                    p => p.Id,
                    post => post.Id,
                    (post, blog) => new { Post = post, BlogName = blog.Blog.Name })
            })
            .ToList();

        foreach (var userPost in userPosts)
        {
            Console.WriteLine($"{userPost.User.Username}'s Posts:");
            foreach (var post in userPost.Posts)
            {
                Console.WriteLine($"  {post.Post.Title} - {post.BlogName}");
            }
            Console.WriteLine();
        }
    }

    static void ReadTextFile(string filePath, Action<string[]> processFields)
    {
        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split(',');
            if (fields.All(field => !string.IsNullOrWhiteSpace(field)))
            {
                processFields(fields);
            }
        }
    }
}
