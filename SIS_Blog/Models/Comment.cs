using System;
using System.Collections.Generic;

namespace SIS_Blog.Models;

public partial class Comment
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public string CreatedAt { get; set; } = null!;

    public string UpdatedAt { get; set; } = null!;

    public int UserId { get; set; }

    public int BlogpostId { get; set; }

    // legacy aliases
    public int User_Id
    {
        get => UserId;
        set => UserId = value;
    }

    public int Blogpost_Id
    {
        get => BlogpostId;
        set => BlogpostId = value;
    }

    public virtual Post Blogpost { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
