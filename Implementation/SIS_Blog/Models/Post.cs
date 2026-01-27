using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIS_Blog.Models;

public partial class Post
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string CreatedAt { get; set; } = null!;

    public string UpdatedAt { get; set; } = null!;

    public int UserId { get; set; }

    [NotMapped]
    public int User_Id
    {
        get => UserId;
        set => UserId = value;
    }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual User User { get; set; } = null!;
}
